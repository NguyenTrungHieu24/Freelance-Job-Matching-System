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


const hideSkeleton = (name) => {
    $(`#${name}Loading`).addClass("d-none");
    $(`#${name}Content`).removeClass("d-none");
};

const showSkeletons = (names = []) => {
    for(let name of names) {
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

    hideSkeleton("jobs");
    $("#totalJobs").text(data.jobStats.totalJobs);
    $("#activeJobs").text(`${data.jobStats.activeJobs} active jobs`);

    hideSkeleton("applications");
    $("#totalApplications").text(data.applicationStats.totalApplications);
    $("#matchRate").text(`${data.applicationStats.matchRate}% match rate`);

    hideSkeleton("revenue");
    $("#totalRevenue").text(`$${data.revenueStats.totalRevenue}`);
    $("#monthlyRevenue").text(`$${data.revenueStats.revenueInRange} this month`);
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