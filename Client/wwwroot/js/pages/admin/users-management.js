$(document).ready(() => {
    $(".js-select-roles").select2({
        placeholder: 'Select roles',
        multiple: true,
        theme: "classic"
    });


    window.changeUserStatus = (user_id) => {
        $.ajax({
            "url": ""
        });
    }
});
