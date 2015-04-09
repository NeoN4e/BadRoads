if (localStorage.option !== undefined) {
            $('select[name="lang"]').prop('selectedIndex', localStorage.option);
        }
        $('select[name="lang"]').change(function () {
            localStorage.option = this.selectedIndex;
        });