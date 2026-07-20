$(document).ready(() => {
    $(".js-select-roles").select2({
        placeholder: 'Select roles',
        multiple: true,
        theme: "classic"
    });

    window.reloadPage = (response) => {
        location.reload();
    };
});
