window.ShowToastr = (type, message) => {
    toastr.options = {
        "closeButton": false,
        "debug": false,
        "newestOnTop": false,
        "progressBar": false,
        "positionClass": "toast-top-right",
        "preventDuplicates": true,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };
   // toastr.success('My name is Inigo Montoya. You killed my father, prepare to die!');

  
    if (type === "success") {
        toastr.success(message, 'Success', { timeOut: 5000, positionClas: "toast-top-right"});
    }
    if (type === "error") {
        toastr.error(message, 'Error', { timeOut: 5000, positionClas: "toast-top-right" });
    }
    if (type === "warning") {
        toastr.error(message, 'Warning', { timeOut: 5000, positionClas: "toast-top-right" });
    }
    if (type === "attributeLimit") {
        toastr.warning(message, 'Attribute Limit', { timeOut: 5000, positionClas: "toast-top-right" });
    }
    if (type === "baseSkillLimit") {
        toastr.warning(message, 'Base Skill Limit', { timeOut: 5000, positionClas: "toast-top-right" });
    }
    if (type === "specialSkillLimit") {
        toastr.warning(message, 'Special Skill Limit', { timeOut: 5000, positionClas: "toast-top-right" });
    }
}

window.ShowSweetAlert = (type, message) => {

    if (type === "success") {
        Swal.fire({
            title: "Succes Notification",
            text: message,
            icon: "success"
        });

    }
    if (type === "error") {
        Swal.fire({
            title: "Failure Notification",
            text: message,
            icon: "error"
        });
    }
}

function ShowDeleteConfirmationModal() {
    $('#deleteConfirmationModal').modal('show');
}

function HideDeleteConfirmationModal() {
    $('#deleteConfirmationModal').modal('hide');
}

function ShowLeavePageModal() {
    $('#leavePageModal').modal('show');
}

function HideLeavePageModal() {
    $('#leavePageModal').modal('hide');
}

function EditKeyDown(id) {
    document.getElementById(id).addEventListener("keydown", function (e) {
        if (e.key == "Enter") {
            e.stopPropagation();
        }
    });
}

function ResizeTextArea(id) {
    var el = document.getElementById(id);
    if (el) {
        el.style.height = "5px";
        el.style.height = (el.scrollHeight + 5) + "px";
    }
    return true;
}
function ResizeRichTextArea(id) {
    var el = document.getElementById(id);
    if (el) {
        el.style.height = "5px";
        el.style.height = (el.scrollHeight + 5) + "px";
        el.style.border = "none";
    }
    return true;
}

window.ScrollToBottom = (elementName) => {
    element = document.getElementById(elementName);

    if (element) {
        element.scrollTop = element.scrollHeight - element.clientHeight;
    }
}

window.GetWindowWidth = function () {
    return window.innerWidth;
};