(function () {
    if (!window.Quill) {
        return;
    }

    var Keyboard = Quill.import('modules/keyboard');
    if (Keyboard && Keyboard.DEFAULTS && Keyboard.DEFAULTS.bindings) {
        delete Keyboard.DEFAULTS.bindings['list autofill'];
    }

    var SPACE_KEY_CODE = 32;

    function disableListAutoformatOnSpace(quill) {
        if (!quill || !quill.keyboard || !quill.keyboard.bindings) {
            return;
        }

        var bindings = quill.keyboard.bindings[SPACE_KEY_CODE];
        if (!bindings) {
            return;
        }

        quill.keyboard.bindings[SPACE_KEY_CODE] = bindings.filter(function (binding) {
            if (!binding.prefix) {
                return true;
            }

            var prefixSource = binding.prefix.source || String(binding.prefix);
            return !/(\d+\.|-|\*|\[ ?\]|\[x\])/.test(prefixSource);
        });
    }

    if (window.QuillFunctions && window.QuillFunctions.createQuill) {
        var originalCreateQuill = window.QuillFunctions.createQuill;
        window.QuillFunctions.createQuill = function () {
            originalCreateQuill.apply(this, arguments);
            var quillElement = arguments[0];
            if (quillElement && quillElement.__quill) {
                disableListAutoformatOnSpace(quillElement.__quill);
            }
        };
    }
})();
