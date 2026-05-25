let userGrowthChart;
let jobGrowthChart;


document.addEventListener("DOMContentLoaded", async function () {

    await loadDashboard();

    document.getElementById("dashboardRange")
        .addEventListener("change", async function () {

            showSkeletons(["users", "jobs", "applications", "revenue", "userGrowth", "jobGrowth", "recentUsers", "recentJobs"]);

            await loadDashboard();
        });
});

/**
 * @desc Random array
 * @param {any} l Array length
 * @param {any} m max value of array item
 * @returns
 */
const randArr = (l = 10, m = 100) => Array.from(
    { length: l },
    () => Math.floor(Math.random() * m) + 1
);

const hideSkeleton = (name) => {
    $(`#${name}Loading`).addClass("d-none");
    $(`#${name}Content`).removeClass("d-none");
};

const showSkeletons = (names = []) => {
    for (let name of names) {
        showSkeleton(name);
    }
}

const showSkeleton = (name) => {
    $(`#${name}Loading`).removeClass("d-none");
    $(`#${name}Content`).addClass("d-none");
};

async function loadDashboard() {

    const range = $("#dashboardRange").val();

    await Promise.all([
        loadOverview(range),
        loadUserGrowthChart(range),
        loadJobGrowthChart(range),
        loadRecentUsers(),
        loadRecentJobs()
    ]);
}

async function loadOverview(range) {

    const response = await fetch(
        `/admin/dashboard/overview?range=${range}`
    );

    const data = await response.json();

    hideSkeleton("users");
    $("#totalUsers").text(data.userStats.totalUsers);
    $("#newUsers").text(`+${data.userStats.totalNewUsers} new users`);
    renderMiniLineChart('#miniChartUsers', {
        data: data.userStats?.miniChart ?? randArr()
    });

    hideSkeleton("jobs");
    $("#totalJobs").text(data.jobStats.totalJobs);
    $("#activeJobs").text(`${data.jobStats.activeJobs} active jobs`);
    renderMiniLineChart('#miniChartJobs', {
        data: data.jobStats?.miniChart ?? randArr()
    });

    hideSkeleton("applications");
    $("#totalApplications").text(data.applicationStats.totalApplications);
    $("#matchRate").text(`${data.applicationStats.matchRate}% match rate`);
    renderMiniLineChart('#miniChartApplications', {
        data: data.applicationStats?.miniChart ?? randArr()
    });

    hideSkeleton("revenue");
    $("#totalRevenue").text(`$${data.revenueStats.totalRevenue}`);
    $("#monthlyRevenue").text(`$${data.revenueStats.revenueInRange} this month`);
    renderMiniLineChart('#miniChartRevenue', {
        data: data.revenueStats?.miniChart ?? randArr()
    });
}

async function loadUserGrowthChart(range) {

    const response = await fetch(`/admin/dashboard/user-growth?range=${range}`);

    const data = await response.json();

    const labels = data.map(x => x.label);

    const freelancers = data.map(x => x.freelancers);

    const employers = data.map(x => x.employers);

    if (userGrowthChart) {
        userGrowthChart.destroy();
    }

    const ctx = document.getElementById("userGrowthContent");

    userGrowthChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Freelancers',
                    data: freelancers
                },
                {
                    label: 'Employers',
                    data: employers
                }
            ]
        }
    });

    hideSkeleton("userGrowth");
}

async function loadJobGrowthChart(range) {

    const response = await fetch(
        `/admin/dashboard/job-growth?range=${range}`
    );

    const data = await response.json();

    const labels = data.map(x => x.label);

    const jobs = data.map(x => x.jobs);

    const applications = data.map(x => x.applications);

    if (jobGrowthChart) {
        jobGrowthChart.destroy();
    }

    const ctx =
        document.getElementById("jobGrowthContent");

    jobGrowthChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Jobs',
                    data: jobs
                },
                {
                    label: 'Applications',
                    data: applications
                }
            ]
        }
    });

    hideSkeleton("jobGrowth");

}

async function loadRecentUsers() {

    const response = await fetch(`/admin/dashboard/recent-users`);

    const data = await response.json();

    let html = '';

    data.forEach(user => {

        html += `
            <tr>
                <td>${user.fullName}</td>
                <td>${user.role}</td>
                <td>${formatDate(user.createdAt)}</td>
            </tr>
        `;
    });

    $("#recentUsersContent").html(html);

    hideSkeleton("recentUsers");

}

async function loadRecentJobs() {

    const response = await fetch(`/admin/dashboard/recent-jobs`);

    const data = await response.json();

    let html = '';

    data.forEach(job => {

        html += `
            <tr>
                <td>${job.title}</td>
                <td>${job.employerName}</td>
                <td>${formatDate(job.createdAt)}</td>
            </tr>
        `;
    });

    $("#recentJobsContent").html(html);
    hideSkeleton("recentJobs");
}

function formatDate(dateString) {

    const date = new Date(dateString);

    return date.toLocaleDateString();
}

let miniCharts = new Map();

/**
 * @desc Render Mini chart
 * @param {string} elem_id Dom element id
 * @param {{data: [], color?: string, background?: string}} options
 * @returns
 */
const renderMiniLineChart = (elem_id, options) => {
    if (miniCharts.has(elem_id)) {
        miniCharts.get(elem_id).destroy();
    }

    miniCharts.set(elem_id,
        new Chart(
            $(elem_id).get(0),
            {
                type: 'line',
                data: {
                    labels: options.data.map(e => ''),
                    datasets: [
                        {
                            data: options.data,
                            borderWidth: 2,
                            tension: 0.4,
                            pointRadius: 0,
                            fill: true,

                            borderColor: options?.color ?? "#139139",

                            backgroundColor: options?.background ?? 'rgba(22,163,74,0.15)',
                        }
                    ]
                },
                options: {
                    plugins: {
                        legend: {
                            display: false
                        }
                    },
                    scales: {
                        x: {
                            display: false
                        },
                        y: {
                            display: false
                        }
                    }
                }
            }
        )
    );
}