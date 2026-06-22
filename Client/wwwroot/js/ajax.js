window.loadAjax = function () {
    console.log("Register ajax event");

    $(document).on('click', '.ajax-postable', async function (e) {

        e.preventDefault();

        const $trigger = $(this);

        if ($trigger.data('loading')) {
            return;
        }

        const confirmText = $trigger.data('confirm');
        const extraText = $trigger.data('extra');

        if (confirmText) {
            let check = await Swal.fire({
                title: confirmText,
                text: extraText,
                icon: 'info',
                showCancelButton: true,
                customClass: {
                    confirmButton: 'btn btn-danger',
                    cancelButton: 'btn btn-secondary'
                },
                buttonsStyling: false
            });

            if (!check.isConfirmed) {
                return;
            }
        }

        $trigger.data('loading', true);
        $trigger.prop('disabled', true);

        $.ajax({
            url: $trigger.data('ajax'),
            type: 'POST',
            data: $trigger.data('post-data') || {}
        })
            .done((response) => {

                if (response?.message) {
                    toastr.success(response.message);
                }

                const callback = $trigger.data('success');

                if (callback && typeof window[callback] === 'function') {
                    window[callback](response, $trigger);
                }

                const redirect = $trigger.data('redirect');

                if (redirect) {
                    location.href = redirect;
                    return;
                }

                if ($trigger.data('reload')) {
                    location.reload();
                }
            })
            .fail((xhr) => {

                toastr.error(
                    xhr.responseJSON?.message ||
                    'Something went wrong!'
                );

            })
            .always(() => {

                $trigger.data('loading', false);
                $trigger.prop('disabled', false);

            });
    });
};