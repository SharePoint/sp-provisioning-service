const defaultSort = 99;

let clickPnpItem = (event) => {

    event.preventDefault();
    event.stopPropagation();

    let curElem = event.currentTarget;

    var items = document.querySelectorAll('.pnp-item');

    items.forEach(element => {

        console.log(element !== curElem);
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

let showAuthOverlay = (event) => {

    event.preventDefault();
    event.stopPropagation();

    var btnPressed = event.currentTarget;
    var iFrame = btnPressed.parentElement.parentElement.parentElement.querySelectorAll('.iframe-container');

    iFrame.forEach(element => {

        element.classList.add('show');

    });

}

// obsolete
// let items = document.querySelectorAll('.pnp-item');

// items.forEach(element => {
//     element.addEventListener('click', clickPnpItem, false);
// });

let addTenantBtns = document.querySelectorAll('.pnp-button');

addTenantBtns.forEach(element => {
    element.addEventListener('click', showAuthOverlay)
})

let closePanel = (event) => {

    event.preventDefault();
    event.stopPropagation();

    event.currentTarget.classList.remove('show');

}

let closeWizards = document.querySelectorAll('.next-step');

closeWizards.forEach(element => {

    element.addEventListener('click', closePanel);

});

let handleBurgerMenu = (event) =>{

    console.log('Burger have been clicked');
    event.preventDefault();
    event.stopPropagation();

    let burgerMenu = document.querySelector('.nav-list');

    if(burgerMenu !== null){

        console.log(burgerMenu);
        if(burgerMenu.classList.contains('show')){

            burgerMenu.classList.remove('show');

        } else {

            burgerMenu.classList.add('show');

        }

    }

}

let burger = document.querySelector('#pnp-btn-burger');
if(burger !== null){
    burger.addEventListener('click', handleBurgerMenu);
}

let startProvisioningFlow = event => {

    event.preventDefault();
    event.stopPropagation();

    var btnPressed = event.currentTarget;
    var provisionUrl = btnPressed.dataset.provisionUrl;

    console.log(provisionUrl);

    window.open(provisionUrl);
};

let provisionBtns = document.querySelectorAll('.pnp-button');

provisionBtns.forEach(element => {
    element.addEventListener('click', startProvisioningFlow);
});
