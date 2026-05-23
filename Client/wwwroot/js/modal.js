window.loadModal = function () {
    console.log("Register modal event")
    $(document).on('click', 'a.has-link_modal', async function (e) {

        e.preventDefault();

        const link = $(this);

        if (link.data('loading')) return;
        link.data('loading', true);

        let href = link.attr('href') || link.data('href');

        if (!href || href === '#') {
            link.data('loading', false);
            return;
        }

        $('.modal').modal('hide');

        let modalId = link.data('modal-id');
        let modalEl = modalId ? $(`#${modalId}`) : null;

        if (!link.hasClass('always_load') && modalEl?.length) {
            modalEl.modal('show');
            link.data('loading', false);
            return;
        }

        if (link.hasClass('always_load') && modalEl) {
            modalEl.remove();
        }

        UI.ajaxShow();

        try {
            const response = await $.get(href);

            const $modal = $(response);
            $('.js-modal-list').append($modal);

            $modal.modal('show');

            $modal.on('hidden.bs.modal', function () {
                if (link.hasClass('always_load')) {
                    $(this).remove();
                }
            });

        } catch (err) {
            console.error(err);
        } finally {
            UI.ajaxHide();
            link.data('loading', false);
        }
    });
};