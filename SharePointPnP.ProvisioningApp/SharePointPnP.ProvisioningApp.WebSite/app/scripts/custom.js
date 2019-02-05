// *******************************************
// Toggle Control implementation
// *******************************************

var toggleEvent = new Event('toggleClicked');

// changes the state of a toggle between On and Off
function toggleState(toggle, dispatchEvent) {

    if (toggle.classList.contains('is-disabled')) {
        return;
    }

    // toggle is-selected class on the toggle element
    var toggleFields = toggle.getElementsByClassName('ms-Toggle-field');
    if (toggleFields.length > 0) {
        toggleFields[0].classList.toggle('is-selected');
    }

    // show or hide the "On" status accordingly
    var labelOn = toggle.getElementsByClassName('ms-Label--on');
    if (labelOn.length > 0) {
        if (toggleFields[0].classList.contains('is-selected')) {
            labelOn[0].style.display = 'block';
        }
        else {
            labelOn[0].style.display = 'none';
        }
    }

    // show or hide the "Off" status accordingly
    var labelOff = toggle.getElementsByClassName('ms-Label--off');
    if (labelOff.length > 0) {
        if (!toggleFields[0].classList.contains('is-selected')) {
            labelOff[0].style.display = 'block';
        }
        else {
            labelOff[0].style.display = 'none';
        }
    }

    if (dispatchEvent) {
        toggle.dispatchEvent(toggleEvent);
    }
}

// disables a toggle field
function disableToggle(toggle) {
    if (!toggle.classList.contains('is-disabled')) {
        toggle.classList.toggle('is-disabled');
    }
}

// enables a toggle field
function enableToggle(toggle) {
    if (toggle.classList.contains('is-disabled')) {
        toggle.classList.toggle('is-disabled');
    }
}

// determines whether the toggle is selected or not
function toggleIsSelected(toggle) {

    // if the toggle is disabled, it implies that it is not selected
    if (toggle.classList.contains('is-disabled')) {
        return (false);
    }

    // otherwise double-check if it is selected
    var toggleFields = toggle.getElementsByClassName('ms-Toggle-field');
    if (toggleFields.length > 0) {
        return (toggleFields[0].classList.contains('is-selected'));
    }

    // or in whatever else scenario, return false (not selected)
    return (false);
}

document.addEventListener('DOMContentLoaded', function (e) {
    var toggles = document.querySelectorAll('.ms-Toggle');
    for (var i = 0; i < toggles.length; i++) {

        toggles[i].addEventListener('click', function (e) {

            // block event bubbling
            e.preventDefault();

            // toggle the state of the toggle element
            toggleState(e.currentTarget, true);

            // cancel the current event
            return (false);
        },
        true // propagate the event to the children elements
        );
    };
});
