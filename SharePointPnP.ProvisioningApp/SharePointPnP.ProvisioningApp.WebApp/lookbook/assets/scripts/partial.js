//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
var isOpen = "is-open";
var isActive = "isActive";

$(function () {
    function isBigScreen() {
        var width = $(window).width();
        return (width > 767);
    }

    

    $.post("./CategoriesMenu" + window.location.search, null, function (html) {
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
            if (!$(event.target).parent().hasClass("menuButton") && $(event.target).siblings(".is-open").length == 0) {
                $("." + isOpen).each((i, m) => {
                    $(m).removeClass(isOpen);
                });
            }
        } else {
            var parentMenu = $(event.target).parents(".menu");
            if ($(parentMenu).length == 0) {
                $("." + isOpen).each((i, m) => {
                    $(m).removeClass(isOpen);
                });
            }
        }
    });

    $(".microsoftNav").click(function (e) {
        var menu = $(".megamenu");
        if ($(menu).hasClass(isOpen)) {
            // Close the menu
            $(menu).removeClass(isOpen);
        }
        else {
            // Close all the menu
            $("." + isOpen).each((j, openMenu) => {
                $(openMenu).removeClass(isOpen);
            })

            // Open the menu
            $(menu).addClass(isOpen);
        }
    });

    $(".hamburger-button").click(function (e) {
        var hamburgerButton = $(".hamburger-button");
        var menu = $(".megamenu");
        var button = $(".microsoftNav");
        var navPanel = $(".right-nav").children("nav.menu");

        if ($(navPanel).hasClass(isOpen)) {
            // Close the menu
            $(navPanel).removeClass(isOpen);
            $(button).removeClass(isOpen);
            $(menu).removeClass(isOpen);
            $(hamburgerButton).removeClass(isOpen);
        }
        else {
            // Close all the menu
            $("." + isOpen).each((j, openMenu) => {
                $(openMenu).removeClass(isOpen);
            })

            // Open the menu
            $(navPanel).addClass(isOpen);
            $(button).addClass(isOpen);
            $(menu).addClass(isOpen);
            $(hamburgerButton).addClass(isOpen);
        }
    });

    $(".megamenu-panel").find("button").click(function (e) {
        var button = $(e.currentTarget);
        var menuPanel = $(e.currentTarget).siblings(".menu-panel");

        if ($(button).hasClass(isOpen)) {
            $(button).removeClass(isOpen);
            $(menuPanel).removeClass(isOpen);
        }
        else {
            $(button).addClass(isOpen);
            $(menuPanel).addClass(isOpen);
        }
    });
});

document.addEventListener("DOMContentLoaded", function (event) {

    // Get the currently connected username, if any
    fetch("/tenant/UserProfile/Username")
        .then(data => { return (data.json()); })
        .then(username => {
            var usernameField = document.body.querySelector(".pnp-username");
            var signInMenu = $("#sign-in-menu");
            var signedInMenu = $("#signed-in-menu");

            if (username) {
                if (usernameField) {
                    usernameField.textContent = username;
                }
                if (signInMenu) {
                    signInMenu.hide();
                }
                if (signedInMenu) {
                    signedInMenu.show();
                }
            }
            else {
                if (usernameField) {
                    usernameField.textContent = "";
                }
                if (signInMenu) {
                    signInMenu.show();
                }
                if (signedInMenu) {
                    signedInMenu.hide();
                }
            }
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