window.ShowToastr = (type, message) => {
    toastr.options = {
        "closeButton": false,
        "debug": false,
        "newestOnTop": false,
        "progressBar": false,
        "positionClass": "toast-bottom-right",
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
        toastr.success(message, 'Success', { timeOut: 5000, positionClas: "toast-bottom-right"});
    }
    if (type === "error") {
        toastr.error(message, 'Error', { timeOut: 5000, positionClas: "toast-bottom-right" });
    }
    if (type === "warning") {
        toastr.error(message, 'Warning', { timeOut: 5000, positionClas: "toast-bottom-right" });
    }
    if (type === "attributeLimit") {
        toastr.warning(message, 'Attribute Limit', { timeOut: 5000, positionClas: "toast-bottom-right" });
    }
    if (type === "baseSkillLimit") {
        toastr.warning(message, 'Base Skill Limit', { timeOut: 5000, positionClas: "toast-bottom-right" });
    }
    if (type === "specialSkillLimit") {
        toastr.warning(message, 'Special Skill Limit', { timeOut: 5000, positionClas: "toast-bottom-right" });
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
    $('.modal-backdrop').remove();
    $('body').removeClass('modal-open').css('overflow', '').css('padding-right', '');
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
window.ScrollToElement = (elementName) => {
    element = document.getElementById(elementName);

    if (element) {
        element.scrollIntoView({ behavior: 'smooth' })
    }
}

window.GetWindowWidth = function () {
    return window.innerWidth;
};

window.DownloadTextFile = (fileName, content) => {
    const blob = new Blob([content], { type: 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);
    URL.revokeObjectURL(url);
};

window.uploadPortraitDataUrl = async (dataUrl, folder) => {
    const blob = await fetch(dataUrl).then(response => response.blob());
    const formData = new FormData();
    formData.append('file', blob, 'portrait.webp');

    const uploadResponse = await fetch(`/api/PortraitUpload?folder=${encodeURIComponent(folder)}`, {
        method: 'POST',
        body: formData,
        credentials: 'include'
    });

    if (!uploadResponse.ok) {
        const errorText = await uploadResponse.text();
        throw new Error(errorText || 'Portrait upload failed.');
    }

    const result = await uploadResponse.json();
    return result.url;
};

window.getEllipseImage = (sourceCanvas) => {
    const newCanvas = document.createElement('canvas');
    const ctxCanvas = newCanvas.getContext('2d');
    const widthCanvas = sourceCanvas.width;
    const heightCanvas = sourceCanvas.height;

    newCanvas.width = widthCanvas;
    newCanvas.height = heightCanvas;
    ctxCanvas.imageSmoothingEnabled = true;

    ctxCanvas.drawImage(sourceCanvas, 0, 0, widthCanvas, heightCanvas);

    ctxCanvas.globalCompositeOperation = 'destination-in';
    ctxCanvas.beginPath();
    ctxCanvas.ellipse(
        widthCanvas / 2,
        heightCanvas / 2,
        widthCanvas / 2,
        heightCanvas / 2,
        0,
        0,
        2 * Math.PI,
        true
    );
    ctxCanvas.fill();

    return newCanvas.toDataURL('image/png', 1);
};

// Keep image processing in the browser so large data URLs never cross the Blazor circuit.
window.uploadCroppedPortrait = async (sourceCanvas, folder) => {
    const dataUrl = sourceCanvas.toDataURL('image/webp', 0.9);
    return await window.uploadPortraitDataUrl(dataUrl, folder);
};

window.uploadEllipseIcon = async (sourceCanvas, folder) => {
    const dataUrl = window.getEllipseImage(sourceCanvas);
    return await window.uploadPortraitDataUrl(dataUrl, folder);
};