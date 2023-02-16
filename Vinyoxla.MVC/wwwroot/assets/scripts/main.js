toastr.options = {
    hideDuration: 300,
    timeOut: 2500,
    positionClass: "toast-bottom-right",
    "closeButton": false,
    "debug": false,
    "newestOnTop": true,
    "progressBar": false,
    "preventDuplicates": false,
    "onclick": null,
    "showEasing": "swing",
    "hideEasing": "linear",
    "showMethod": "fadeIn",
    "hideMethod": "fadeOut"
}

$(document).ready(function () {

    $('.preloaderdiv').addClass('d-none')

    if ($(document).width() > 576) {
        $('#vincodeinput').focus()
        $('#phonenumber').focus()
        $('#phoneno').focus()
        $('#cardholder').focus()
    } else {
        $('.reklamhref').attr('href', 'https://www.instagram.com/garantauto.az/?hl=en')
    }

    //#region prevent letters and nonnumeric

    $(document).on('input', '#phonenumber, #phoneno, #code, #amounttopup', function (e) {
        if (!/^[0-9]+$/.test($(this).val())) {
            $(this).val($(this).val().slice(0, -1))
        }
    })

    //#endregion prevent letters and nonnumeric

    // -------------------------- main page

    //#region input toUpperCase in vincode input

    $(document).on('input keyup', '#vincodeinput', function () {
        let value = $(this).val();
        $(this).val(value.toUpperCase());

        if (value.length > 0) {
            $('.copyspan').hide();
            $('.deleteinp').show();
        }
        else {
            $('.copyspan').show();
            $('.deleteinp').hide();
        }

        let regex = /\b[(A-H|J-N|P|R-Z|0-9)]{17}\b/

        if (regex.test(value.toUpperCase())) {
            $('#vincodebutton').removeClass('noClick')
        }
        else {
            $('#vincodebutton').addClass('noClick')
        }
    });

    //#endregion input toUpperCase in vincode input

    //#region delete input value

    $(document).on('click', '.deleteinp', function () {
        $('#vincodeinput').val("");
        $('#vincodebutton').addClass('noClick');
        $('.copyspan').show();
        $('.deleteinp').hide();
    })

    //#endregion delete input value

    //#region copy paste

    $(document).on('click pointerdown pointerup', '.copyspan', function () {

        navigator.clipboard.readText()
            .then((text) => {

                let regex = /\b[(A-H|J-N|P|R-Z|0-9)]{17}\b/

                if (regex.test(text.trim().toUpperCase())) {
                    $('#vincodeinput').val(text.trim().toUpperCase())
                    $('#vincodebutton').removeClass('noClick')
                    $('.copyspan').hide();
                    $('.deleteinp').show();
                }
            });
    })

    //#endregion copy paste

    //#region fetch

    $(document).on('submit', '#vincodeform', function (e) {
        e.preventDefault();
        const formData = new FormData(e.target);

        let vinCode = formData.get('vincodeinput');

        let regex = /\b[(A-H|J-N|P|R-Z|0-9)]{17}\b/

        if (!regex.test(vinCode.trim().toUpperCase())) {
            toastr.error('Daxil edilən VİN kodun strukturu səhvdir!')
            return;
        }

        $('.preloaderdiv').removeClass('d-none');
        $('.resultsContainer').addClass('d-none');
        $('#carfaxContainer').remove();
        $('#photosApiResult').remove();
        $('.reklam').remove();

        let url = $(this).attr('action');

        url = url + '?vinCode=' + vinCode;

        axios.get(url)
            .then(function (res) {

                $('#resultscontainer').prepend(res.data)

                $('.resultsContainer').removeClass('d-none')

                $('.preloaderdiv').addClass('d-none');

                if ($(document).width() < 576) {
                    $(document).scrollTop(600)
                }
                else if ($(document).width() > 1200) {
                    $(document).scrollTop(1500);
                }
                else {
                    $('#resultscontainer')[0].scrollIntoView()
                }
            })
            .catch(function (err) {
                $('.preloaderdiv').addClass('d-none');

                toastr.error('Daxil edilən VİN kod tapılmadı!')
                return;
            })
    })

    //#endregion fetch

    // -------------------------- main page

    // --------------------------

    // -------------------------- login page

    //#region login input focus on click on span

    $(document).on('click', '.kodstrani', function () {
        $('#phoneno').focus();
    })

    //#endregion login input focus on click on span

    //#region login input submit

    $(document).on('submit', '#loginform', function (e) {
        const formData = new FormData(e.target);

        let phoneno = formData.get('PhoneNumber');
        let code = formData.get('Code');

        if (!(phoneno.startsWith("50") ||
            phoneno.startsWith("10") ||
            phoneno.startsWith("51") ||
            phoneno.startsWith("70") ||
            phoneno.startsWith("77") ||
            phoneno.startsWith("99") ||
            phoneno.startsWith("55"))) {
            e.preventDefault();
            toastr.error('Nömrə səhvdir.');
            return;
        }

        if (phoneno.length != 9) {
            e.preventDefault();
            toastr.error('Məlumatlar səhvdir.');
            return;
        }

        if ($('#kodgelennensonra').find('#code').length != 1) {
            e.preventDefault();

            $('.preloaderdiv').removeClass('d-none')
            $('#sendcode').html("Daxil ol");
            $('#phoneno').prop('readonly', 'true');

            let obj = {
                phoneNumber: $('#phoneno').val()
            }

            axios.post("/Account/SendCode", obj)
                .then(function (res) {
                    $('.preloaderdiv').addClass('d-none')
                    $('#kodgelennensonra').append(res.data)
                    $('#code').focus();
                })
                .catch(function (err) {
                    $('.preloaderdiv').addClass('d-none')
                    toastr.error('Daxil edilən məlumatlar səhvdir!')
                    $('#sendcode').html("Kodu göndər");
                    $('#phoneno').prop('readonly', 'false');
                    $('#phoneno').focus();
                    return;
                })
            return;
        }

        $('.preloaderdiv').addClass('d-none')

        if (code.length != 4) {

            e.preventDefault();
            toastr.error('Məlumatlar səhvdir.');
            return;
        }
    })

    //#endregion login input submit

    // -------------------------- login page

    // -------------------------- 

    // -------------------------- purchase page

    //#region purchase form

    //$(document).on('submit', '#purchaseForm', function (e) {
    //    const formData = new FormData(e.target);

    //    let cardno = formData.get('CardNo');
    //    let cardholder = formData.get('CardHolder');
    //    let year = formData.get('CardYear');
    //    let month = formData.get('Month');
    //    let cvv = formData.get('CVV');
    //    let phonenumber = formData.get('PhoneNumber');

    //    if (month.length > 2 || year.length > 4 || cvv.length > 3) {
    //        toastr.error('Məlumatlar səhvdir.');
    //        e.preventDefault();
    //        return;
    //    }

    //    if (!/^[0-9]+$/.test(cardno) &&
    //        !/^[0-9]+$/.test(year) &&
    //        !/^[0-9]+$/.test(month) &&
    //        !/^[0-9]+$/.test(cvv)) {
    //        toastr.error('Məlumatlar səhvdir.');
    //        e.preventDefault();
    //        return;
    //    }

    //    if (phonenumber != null) {
    //        if (!(phonenumber.startsWith("50") ||
    //            phonenumber.startsWith("10") ||
    //            phonenumber.startsWith("51") ||
    //            phonenumber.startsWith("70") ||
    //            phonenumber.startsWith("77") ||
    //            phonenumber.startsWith("99") ||
    //            phonenumber.startsWith("55"))) {
    //            toastr.error('Nömrə səhvdir.');
    //            e.preventDefault();
    //            return;
    //        }
    //    }

    //    if (!(cardno.startsWith("2") ||
    //        cardno.startsWith("3") ||
    //        cardno.startsWith("4") ||
    //        cardno.startsWith("5"))) {
    //        toastr.error('Kartın nömrəsi səhvdir.');
    //        e.preventDefault();
    //        return;
    //    }

    //    if (month > 12) {
    //        toastr.error('Ay səhvdir.');
    //        e.preventDefault();
    //        return;
    //    }

    //    if (!/^[a-zA-Z\s]*$/.test(cardholder)) {
    //        toastr.error('Ad soyad səhvdir.');
    //        e.preventDefault();
    //        return;
    //    }

    //    let date = new Date;

    //    let cardDate = '01/' + month.trim() + '/' + year.trim();

    //    let convertedDate = new Date(cardDate)

    //    if (convertedDate.getTime() < date.getTime()) {
    //        toastr.error('Kartın son tarixi səhvdir.');
    //        e.preventDefault();
    //        return;
    //    }
    //});

    //#endregion purchase form

    //#region input toUpperCase

    //$(document).on('input keyup', '#cardholder', function () {
    //    let value = $(this).val();
    //    $(this).val(value.toUpperCase());

    //    let regex = /^[a-zA-Z\s]*$/;

    //    if (!regex.test(value)) {
    //        $(this).val($(this).val().slice(0, -1))
    //    }
    //});

    //#endregion input toUpperCase

    //#region visa or mastercard

    //$(document).on('input keyup', '#cardno', function () {
    //    let value = $(this).val();

    //    if (value.charAt(0) == 4) {
    //        $('.visabox').addClass('odu');
    //        $('.mastercardbox').removeClass('odu');
    //    }

    //    if (value.charAt(0) == 2 || value.charAt(0) == 5) {
    //        $('.mastercardbox').addClass('odu');
    //        $('.visabox').removeClass('odu');
    //    }

    //    if (value.length == 0) {
    //        $('.visabox').removeClass('odu');
    //        $('.mastercardbox').removeClass('odu');
    //    }
    //});

    //#endregion visa or mastercard

    //#region change input on input complete

    //$(document).on('input', '#cardno', function () {

    //    if ($(this).val().length == 16) {
    //        if ($('#month').val().length != 2) {
    //            $('#month').focus()
    //        }
    //    }
    //})

    //$(document).on('input', '#month', function () {

    //    if ($(this).val().length == 2) {
    //        if ($('#year').val().length != 4) {
    //            $('#year').focus()
    //        }
    //    }
    //})

    //$(document).on('input', '#year', function () {

    //    if ($(this).val().length == 4) {
    //        if ($('#cvv').val().length != 3) {
    //            $('#cvv').focus()
    //        }
    //    }
    //})

    //$(document).on('input', '#cvv', function () {

    //    if ($(this).val().length == 3) {
    //        if ($('#phonenumber').val().length != 9) {
    //            $('#phonenumber').focus()
    //        }
    //    }
    //})

    //#endregion change input on input complete

    // -------------------------- purchase page

    // -------------------------- 

    // -------------------------- account page

    //#region input to uppercase

    $(document).on('input keyup', '#search', function () {
        let value = $(this).val();
        $(this).val(value.toUpperCase());
    });

    //#endregion input to uppercase

    //#region search input

    $(document).on('input', '#search', function () {
        let value = $(this).val();
    });

    //#endregion search input

    //#region sort backend

    if (localStorage.getItem('sort') == null) {

        localStorage.setItem('sort', JSON.stringify([]));

        let obj = {
            vin: $('#search').val(),
            showcount: $('#showcount').find(":selected").val(),
            sortbydate: $('#sortbydate').find(":selected").val(),
            page: 1
        }

        localStorage.setItem('sort', JSON.stringify(obj));
    }

    if (window.location.href.indexOf("Profile") < 0) {
        localStorage.clear();
    }

    $(document).on('keyup', '#search', function () {

        let sort = JSON.parse(localStorage.getItem('sort'));

        sort.vin = $(this).val();

        localStorage.setItem('sort', JSON.stringify(sort));

        MakeRequest(sort)
    });

    $(document).on('change', '#sortbydate, #showcount', function () {

        let sort = JSON.parse(localStorage.getItem('sort'));

        sort.showcount = $('#showcount').find(":selected").val();
        sort.sortbydate = $('#sortbydate').find(":selected").val();

        localStorage.setItem('sort', JSON.stringify(sort));

        MakeRequest(sort)
    });

    $(document).on('click', '.page-item', function () {

        let sort = JSON.parse(localStorage.getItem('sort'));

        sort.page = $(this).data("page");

        localStorage.setItem('sort', JSON.stringify(sort));

        MakeRequest(sort)
    });

    let MakeRequest = ({ vin, page, sortbydate, showcount }) => {
        axios.get("/Account/Sort", {
            params: {
                vin,
                page,
                sortbydate,
                showcount
            }
        })
            .then(function (res) {

                $('#accountvins').html("")
                $("#accountvins").html(res.data)
            })
    }

    //#endregion sort backend

    //#region balance form

    $(document).on('click', '#animatetopup', function () {

        $(this).addClass('animatetopupbtn');
        $(this).html("Balans");
        $('.topupbalanceform').removeClass('animatetopupbtn');
    });

    //#endregion balance form

    // -------------------------- account page

    // -------------------------- 

    // -------------------------- images page

    //#region download image

    $(document).on('click', '.downloadimg', function (e) {
        let url = $(this).data('url')

        fetch(url, {
            mode: 'cors',
        })
            .then(response => response.blob())
            .then(blob => {
                let blobUrl = window.URL.createObjectURL(blob);
                let a = document.createElement('a');
                a.download = url.replace(/^.*[\\\/]/, '');
                a.href = blobUrl;
                document.body.appendChild(a);
                a.click();
                a.remove();
            })
    })

    //#endregion download image

    //#region slider in images page

    $('.sliderimages1').slick({
        dots: false,
        infinite: true,
        asNavFor: '.miniimages1',
        speed: 500,
        prevArrow: $('.prevbtn1'),
        nextArrow: $('.nextbtn1'),
        slidesToShow: 1,
        slidesToScroll: 1,
        responsive: [
            {
                breakpoint: 1024,
                settings: {
                    slidesToShow: 1,
                    slidesToScroll: 1,
                    infinite: true,
                    dots: false
                }
            },
            {
                breakpoint: 576,
                settings: {
                    slidesToShow: 1,
                    slidesToScroll: 1,
                    infinite: true,
                    dots: false
                }
            },
            {
                breakpoint: 480,
                settings: {
                    slidesToShow: 1,
                    slidesToScroll: 1,
                    infinite: true,
                    dots: false
                }
            }
        ]
    });

    $('.miniimages1').slick({
        dots: false,
        infinite: true,
        focusOnSelect: true,
        asNavFor: '.sliderimages1',
        speed: 500,
        prevArrow: $('.prevbtn1'),
        nextArrow: $('.nextbtn1'),
        slidesToShow: 8,
        slidesToScroll: 1,
        responsive: [
            {
                breakpoint: 1024,
                settings: {
                    slidesToShow: 7,
                    slidesToScroll: 1,
                    infinite: true,
                    dots: false
                }
            },
            {
                breakpoint: 576,
                settings: {
                    slidesToShow: 4,
                    slidesToScroll: 1,
                    infinite: true,
                    dots: false
                }
            },
            {
                breakpoint: 480,
                settings: {
                    slidesToShow: 4,
                    slidesToScroll: 1,
                    infinite: true,
                    dots: false
                }
            }
        ]
    });

    $('.sliderimages2').slick({
        dots: false,
        infinite: true,
        speed: 500,
        prevArrow: $('.prevbtn2'),
        nextArrow: $('.nextbtn2'),
        slidesToShow: 1,
        slidesToScroll: 1,
        responsive: [
            {
                breakpoint: 1024,
                settings: {
                    slidesToShow: 1,
                    slidesToScroll: 1,
                    infinite: true,
                    dots: false
                }
            },
            {
                breakpoint: 576,
                settings: {
                    slidesToShow: 1,
                    slidesToScroll: 1,
                    infinite: true,
                    dots: false
                }
            },
            {
                breakpoint: 480,
                settings: {
                    slidesToShow: 1,
                    slidesToScroll: 1,
                    infinite: true,
                    dots: false
                }
            }
        ]
    });

    $('.miniimages2').slick({
        dots: false,
        infinite: true,
        focusOnSelect: true,
        asNavFor: '.sliderimages2',
        speed: 500,
        prevArrow: $('.prevbtn2'),
        nextArrow: $('.nextbtn2'),
        slidesToShow: 8,
        slidesToScroll: 1,
        responsive: [
            {
                breakpoint: 1024,
                settings: {
                    slidesToShow: 7,
                    slidesToScroll: 1,
                    infinite: true,
                    dots: false
                }
            },
            {
                breakpoint: 576,
                settings: {
                    slidesToShow: 4,
                    slidesToScroll: 1,
                    infinite: true,
                    dots: false
                }
            },
            {
                breakpoint: 480,
                settings: {
                    slidesToShow: 4,
                    slidesToScroll: 1,
                    infinite: true,
                    dots: false
                }
            }
        ]
    });

    //#endregion slider in images page

    // -------------------------- images page

    // -------------------------- 

    // -------------------------- report page

    //#region print button

    $(document).on('click', '#printbtn', function () {

        window.print();
    });

    //#endregion print button

    // -------------------------- report page

});