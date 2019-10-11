//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//

let startProvisioningFlow = event => {

    event.preventDefault();
    event.stopPropagation();

    var btnPressed = event.currentTarget;
    var provisionUrl = btnPressed.dataset.provisionUrl;

    location.href = provisionUrl;
};

let provisionSlbBtns = document.querySelectorAll('.pnp-start-provisioning');

provisionSlbBtns.forEach(element => {
    element.addEventListener('click', startProvisioningFlow)
})

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

                colorSelector.style.marginTop = marginSetOff + "px";
                colorSelector.style.backgroundColor = "lime";

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