var isOpen = "is-open";
var isActive = "isActive";

document.addEventListener("DOMContentLoaded", function (event) {

    hookMenuEvents();

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

    // Hide the open menu when clicked outside
    document.addEventListener("mouseup", function (event) {
        var openMenus = document.querySelectorAll("." + isOpen);
        [].forEach.call(openMenus, function (m) {
            m.classList.remove(isOpen);
        });
    });
});

function hookMenuEvents() {

    // Header menu
    var menuButtons = document.querySelectorAll(".menuButton");
    for (var i = 0; i < menuButtons.length; i++) {
        menuButtons[i].addEventListener('click', function (e) {
            var menuPanels = e.currentTarget.parentElement.querySelectorAll(".menu-panel");
            [].forEach.call(menuPanels, function (menuPanel) {
                if (menuPanel.classList.contains(isOpen)) {
                    // Close the menu if it is open
                    menuPanel.classList.remove(isOpen);
                }
                else {
                    // Close all the other menu
                    var menus = document.querySelectorAll("." + isOpen);
                    [].forEach.call(menus, function (m) {
                        m.classList.remove(isOpen);
                    });

                    // Open the menu
                    menuPanel.classList.add(isOpen);
                }
            });
        });
    }
}