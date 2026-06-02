@section Scripts {
    <script>
       

            document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', function (e) {

                const target = this.getAttribute('href');

                if (!target || target === '#') return;

                const element = document.querySelector(target);

                if (!element) return;

                e.preventDefault();

                element.scrollIntoView({
                    behavior: 'smooth'
                });
            });
});
    </script>
}