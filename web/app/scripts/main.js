const defaultSort = 99;

let clickPnpItem = (event) => {

    event.preventDefault();
    event.stopPropagation();

    let curElem = event.currentTarget;

    var items = document.querySelectorAll('.pnp-item');

    items.forEach(element => {

        if (element !== curElem) {
            element.classList.remove('show');
        }

    });

    if (curElem.classList.contains('show')) {
        curElem.classList.remove('show');
    } else {
        curElem.classList.add('show');
    }

}

// let showAuthOverlay = (event) => {

//     event.preventDefault();
//     event.stopPropagation();

//     var btnPressed = event.currentTarget;
//     var iFrame = btnPressed.parentElement.parentElement.parentElement.querySelectorAll('.iframe-container');

//     iFrame.forEach(element => {

//         element.classList.add('show');

//     });

// }

// obsolete
// let items = document.querySelectorAll('.pnp-item');

// items.forEach(element => {
//     element.addEventListener('click', clickPnpItem, false);
// });

// let addTenantBtns = document.querySelectorAll('.pnp-button');

// addTenantBtns.forEach(element => {
//     element.addEventListener('click', showAuthOverlay)
// })

// let closePanel = (event) => {

//     event.preventDefault();
//     event.stopPropagation();

//     event.currentTarget.classList.remove('show');

// }

// let closeWizards = document.querySelectorAll('.next-step');

// closeWizards.forEach(element => {

//     element.addEventListener('click', closePanel);

// });

let handleBurgerMenu = (event) => {

    event.preventDefault();
    event.stopPropagation();

    let burgerMenu = document.querySelector('.nav-list');

    if (burgerMenu !== null) {

        if (burgerMenu.classList.contains('show')) {

            burgerMenu.classList.remove('show');

        } else {

            burgerMenu.classList.add('show');

        }

    }

}

const toggleFilter = (event) => {

    event.preventDefault();

    if (event.target.classList.contains('active') === true) {

        event.target.classList.remove('active');

        let filterPanel = document.querySelector('.pnp-filter-panel');

        filterPanel.classList.remove('show');

    } else {

        event.target.classList.add('active');

        let filterPanel = document.querySelector('.pnp-filter-panel');

        filterPanel.classList.add('show');

    }

}

let burger = document.querySelector('#pnp-btn-burger');
if (burger !== null) {
    burger.addEventListener('click', handleBurgerMenu);
}

let filter = document.querySelector('.pnp-filter .pnp-filter-toggle')
if (filter !== null) {
    filter.addEventListener('click', toggleFilter);
}


const colorPickerInit = () => {

    window.addEventListener('keydown', (event) => {

        if (event.keyCode === 13) {
            event.preventDefault();
            return false;
        }

    })

    let colorSeletors = [];

    let colorRegex = /^#[0-9A-F]{6}$/i;

    let colorPickers = document.querySelectorAll('input.pnp-tb[name$=Color]');

    let setColor = (event, closePicker) => {

        let colorIndex = event.target.getAttribute('colorindex');
        let colorValue = event.target.value;
        let sourceId = event.target.id;

        if (colorValue.match(colorRegex)) {

            colorSeletors[colorIndex].set(colorValue);

        } else {

            if (colorSeletors[colorIndex].value !== undefined) {

                event.target.value = colorSeletors[colorIndex].value;

            }

        }

        if (closePicker) {

            let colorPreview = document.querySelector('.pnp-colorpreview[rel=' + sourceId + ']');
            colorPreview.style.backgroundColor = event.target.value;

            colorSeletors[colorIndex].exit();

        }


    }

    for (let i = 0; i < colorPickers.length; i++) {

        // add color index to text input
        colorPickers[i].setAttribute('colorindex', i);

        // create new instance of a color picker
        colorSeletors.push(new CP(colorPickers[i]));

        colorPickers[i].addEventListener('focusout', (event) => {

            setColor(event, true);

        })

        colorPickers[i].addEventListener('focusin', (event) => {

            const colorSelector = document.querySelector('.color-picker');
            const header = document.querySelector('.header-img');

            // remove the header image form top
            let computedStyles = window.getComputedStyle(header);

            /**
             * Overlay style correction
             */
            if (colorSelector !== null && header !== null) {

                let marginSetOff = computedStyles.getPropertyValue('height').replace('px', '') * -1;

                colorSelector.style.marginTop = marginSetOff + 'px';
                colorSelector.style.backgroundColor = 'lime';

            }

        })


        colorPickers[i].addEventListener('keydown', setColor);


        // init all color picker
        colorSeletors[i].on('change', function (color, event) {


            this.source.value = '#' + color;

            let sourceId = this.source.id;

            let colorPreview = document.querySelector('.pnp-colorpreview[rel=' + sourceId + ']');
            colorPreview.style.backgroundColor = '#' + color;

        })

    }
}

colorPickerInit();
