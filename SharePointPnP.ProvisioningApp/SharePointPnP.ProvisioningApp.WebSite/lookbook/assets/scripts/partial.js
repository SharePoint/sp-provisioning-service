var isOpen = "is-open";
var isActive = "isActive";

$(function () {
    function isBigScreen() {
        var width = $(window).width();
        return (width > 767);
    }

    

    $.post("/home/CategoriesMenu" + window.location.search, null, function (html) {
        // Get a reference to the menu
        var menu = $("#packagesMenu");

        // Build the HTML response
        $(html).children().each((i, child) => {
            $(menu).append(child);
        });

        $(".menuButton").each((i, menuElement) => {
            $(menuElement).click(function (e) {
                $(e.currentTarget).siblings(".menu-panel").each((j, menuPanel) => {
                    if ($(menuPanel).hasClass(isOpen)) {
                        // Close the menu if it is open
                        $(menuPanel).removeClass(isOpen);
                    }
                    else {
                        if (isBigScreen()) {
                            // Close all the other menu
                            $("." + isOpen).each((k, openedMenu) => {
                                var responsiveMenu = $(".page-nav").find(".menu");
                                if ($(responsiveMenu)[0] != $(openedMenu)[0]) {
                                    $(openedMenu).removeClass(isOpen);
                                }
                            });
                        }

                        // Open the menu
                        $(menuPanel).addClass(isOpen);
                    }
                });
            });
        });

        $("#packagesMenu").find(".menuButton").each((i, menuElement) => {
            $(menuElement).mouseover(function (e) {
                if(isBigScreen()) {
                    $(e.currentTarget).siblings(".menu-panel").each((j, menuPanel) => {

                        // Close all the other menu
                        $("#packagesMenu").find("." + isOpen).each((k, openedMenu) => {
                            $(openedMenu).removeClass(isOpen);
                        });

                        if (!$(menuPanel).hasClass(isOpen)) {
                            // Open the menu
                            $(menuPanel).addClass(isOpen);
                        }
                    });
                }
            });
        });

        // Responsive menus
        $(".responsiveMenuButton").click(function (e) {
            var menu = $(".page-nav > .menu");
            if ($(menu).hasClass(isOpen)) {
                // Close the menu if it is open
                $("." + isOpen).removeClass(isOpen);
            }
            else {
                // Open the menu
                $(menu).addClass(isOpen);

            }
        });

        $(window).on('resize', function () {
            $(".is-open").removeClass("is-open");
        });
    });

    // Hide the open menu when clicked outside
    $(document).mouseup(function (event) {
        if (isBigScreen()) {
            if (!$(event.target).parent().hasClass("menuButton")) {

                var openMenus = document.querySelectorAll("." + isOpen);
                [].forEach.call(openMenus, function (m) {
                    m.classList.remove(isOpen);
                });
            }
        }
    });
});

document.addEventListener("DOMContentLoaded", function (event) {

    // Microsoft Mega menu
    document.querySelector(".microsoftNav").addEventListener('click', function (e) {
        var menus = document.querySelectorAll(".megamenu");
        [].forEach.call(menus, function (menu) {
            if (menu.classList.contains(isOpen)) {
                // Close the menu
                menu.classList.remove(isOpen);
            }
            else {
                // Close all the menu
                var openMenus = document.querySelectorAll("." + isOpen);
                [].forEach.call(openMenus, function (openMenu) {
                    openMenu.classList.remove(isOpen);
                })

                // Open the menu
                menu.classList.add(isOpen);
            }
        });
    });

    // Categories selection
    var tabsCategories = document.querySelectorAll(".slb-tab-label");
    for (var i = 0; i < tabsCategories.length; i++) {
        tabsCategories[i].addEventListener('click', function (e) {
            var slbTab = e.currentTarget;
            if (slbTab && !slbTab.classList.contains(isActive)) {

                // Clear all the tabs
                var tabs = document.querySelectorAll(".slb-tab-label");
                [].forEach.call(tabs, function (t) {
                    t.classList.remove(isActive);
                });

                // Highlight the currently selected tab
                slbTab.classList.add(isActive);
            }
        });
    }
});