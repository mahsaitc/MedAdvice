jQuery(function($) {
	'use strict';
	$(window).on('scroll', function() {
		if ($(this).scrollTop() > 120) {
			$('.navbar-area').addClass("is-sticky");
		} else {
			$('.navbar-area').removeClass("is-sticky");
		}
	});
	$(window).on('scroll', function() {
		if ($(this).scrollTop() > 120) {
			$('.main-navbar').addClass("is-sticky");
		} else {
			$('.main-navbar').removeClass("is-sticky");
		}
	});
	jQuery('.mean-menu').meanmenu({
		meanScreenWidth: "1200"
	});
	$(function() {
		$('.default-btn').on('mouseenter', function(e) {
			var parentOffset = $(this).offset(),
				relX = e.pageX - parentOffset.left,
				relY = e.pageY - parentOffset.top;
			$(this).find('span').css({
				top: relY,
				left: relX
			})
		}).on('mouseout', function(e) {
			var parentOffset = $(this).offset(),
				relX = e.pageX - parentOffset.left,
				relY = e.pageY - parentOffset.top;
			$(this).find('span').css({
				top: relY,
				left: relX
			})
		});
	});
	$('select').niceSelect();
	$('.popup-youtube').magnificPopup({
		disableOn: 320,
		type: 'iframe',
		mainClass: 'mfp-fade',
		removalDelay: 160,
		preloader: false,
		fixedContentPos: false,
	});
	$('.team-slider').owlCarousel({
		loop: true,
		margin: 20,
		nav: false,
		autoplay: true,
		autoplayHoverPause: true,
		dots: true,
		responsive: {
			0: {
				items: 1,
			},
			600: {
				items: 2,
			},
			1000: {
				items: 3,
			}
		}
	})
	$('.testimonials-slider').owlCarousel({
		items: 1,
		loop: true,
		margin: 50,
		nav: false,
		autoplay: false,
		autoplayHoverPause: true,
		dots: true,
	})
	$('.blog-slider').owlCarousel({
		items: 1,
		loop: true,
		margin: 20,
		nav: false,
		autoplay: true,
		autoplayHoverPause: true,
		dots: true,
		responsive: {
			0: {
				items: 1,
			},
			600: {
				items: 2,
			},
			1000: {
				items: 3,
			}
		}
	})
	$('.dedicated-slider').owlCarousel({
		items: 1,
		loop: true,
		margin: 20,
		nav: false,
		autoplay: true,
		autoplayHoverPause: true,
		dots: true,
		responsive: {
			0: {
				items: 1,
			},
			600: {
				items: 2,
			},
			1000: {
				items: 3,
			}
		}
	})
	$('.treatment-slider').owlCarousel({
		items: 1,
		loop: true,
		margin: 20,
		nav: false,
		autoplay: true,
		autoplayHoverPause: true,
		dots: true,
		responsive: {
			0: {
				items: 1,
			},
			600: {
				items: 2,
			},
			1000: {
				items: 3,
			}
		}
	})
	$('.doctors-slider').owlCarousel({
		items: 1,
		loop: true,
		margin: 20,
		nav: false,
		autoplay: true,
		autoplayHoverPause: true,
		dots: true,
		responsive: {
			0: {
				items: 1,
			},
			600: {
				items: 2,
			},
			1000: {
				items: 3,
			}
		}
	})
	$('.clients-slider').owlCarousel({
		items: 1,
		loop: true,
		margin: 30,
		nav: false,
		autoplay: true,
		autoplayHoverPause: true,
		dots: true,
		responsive: {
			0: {
				items: 1,
			},
			768: {
				items: 2,
			},
			1000: {
				items: 2,
			}
		}
	})
	$('.testimonials-three-slider').owlCarousel({
		items: 1,
		loop: true,
		margin: 20,
		nav: false,
		autoplay: true,
		autoplayHoverPause: true,
		dots: true,
		responsive: {
			0: {
				items: 1,
			},
			600: {
				items: 2,
			},
			1000: {
				items: 3,
			}
		}
	})
	$('.doctors-list-slider').owlCarousel({
		items: 1,
		loop: true,
		margin: 20,
		nav: false,
		autoplay: true,
		autoplayHoverPause: true,
		dots: true,
		responsive: {
			0: {
				items: 1,
			},
			600: {
				items: 2,
			},
			1000: {
				items: 3,
			}
		}
	})
	$('.blog-three-slider').owlCarousel({
		items: 1,
		loop: true,
		margin: 20,
		nav: false,
		autoplay: true,
		autoplayHoverPause: true,
		dots: true,
		responsive: {
			0: {
				items: 1,
			},
			600: {
				items: 2,
			},
			1000: {
				items: 3,
			}
		}
	})
	$('.partner-slider').owlCarousel({
		loop: true,
		dots: false,
		margin: 30,
		nav: false,
		autoplay: true,
		autoplayHoverPause: true,
		responsive: {
			0: {
				items: 2
			},
			576: {
				items: 3
			},
			768: {
				items: 4
			},
			1200: {
				items: 5
			}
		}
	})
	$('.covax-gallery').magnificPopup({
		delegate: 'a',
		type: 'image',
		tLoading: 'Loading image #%curr%...',
		mainClass: 'mfp-img-mobile',
		gallery: {
			enabled: true,
			navigateByImgClick: true,
			preload: [0, 1]
		}
	});
	$('.accordion > li:eq(0) .title').addClass('active').next().slideDown();
	$('.accordion .title').click(function(j) {
		var dropDown = $(this).closest('li').find('.accordion-content');
		$(this).closest('.accordion').find('.accordion-content').not(dropDown).slideUp();
		if ($(this).hasClass('active')) {
			$(this).removeClass('active');
		} else {
			$(this).closest('.accordion').find('.title.active').removeClass('active');
			$(this).addClass('active');
		}
		dropDown.stop(false, true).slideToggle();
		j.preventDefault();
	});
	$('#Container').mixItUp();
	jQuery(window).on('load', function() {
		jQuery(".preloader").fadeOut(500);
	});
	$.datetimepicker.setLocale('pt-BR');
	$('#datetimepicker').datetimepicker();
	$(window).on('scroll', function() {
		if ($(this).scrollTop() > 0) {
			$('.go-top').addClass('active');
		} else {
			$('.go-top').removeClass('active');
		}
	});
	$(function() {
		$(window).on('scroll', function() {
			var scrolled = $(window).scrollTop();
			if (scrolled > 600) $('.go-top').addClass('active');
			if (scrolled < 600) $('.go-top').removeClass('active');
		});
		$('.go-top').on('click', function() {
			$("html, body").animate({
				scrollTop: "0"
			}, 1000);
		});
	});
}(jQuery));