//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
let proceedNext = (event) =>{

    event.preventDefault();
    event.stopPropagation();

    let parentContainer = window.frameElement.parentElement;
    parentContainer.classList.remove('show');
    console.log(parentContainer.nextElementSibling);
    parentContainer.nextElementSibling.classList.add('show');

}

let buttons = document.getElementById('closeAndProceed');

console.log(buttons);

buttons.addEventListener('click', proceedNext);
