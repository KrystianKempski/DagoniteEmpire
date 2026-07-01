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

    if (window.QuillFunctions && window.QuillFunctions.loadQuillHTMLContent) {
        // Keep Blazored's innerHTML loader — clipboard.convert/setContents breaks typing and blockquotes.
        window.QuillFunctions.loadQuillHTMLContent = function (quillElement, quillHTMLContent) {
            var quill = quillElement && quillElement.__quill;
            if (!quill) {
                return;
            }

            quill.root.innerHTML = quillHTMLContent || '';
        };
    }

    window.QuillPostEditor = {
        findQuillRoot: function (containerId) {
            var container = document.getElementById(containerId);
            if (!container) {
                return null;
            }

            var nodes = container.querySelectorAll('div');
            for (var i = 0; i < nodes.length; i++) {
                if (nodes[i].__quill) {
                    return nodes[i];
                }
            }

            return null;
        },

        getQuill: function (containerId) {
            var root = this.findQuillRoot(containerId);
            return root && root.__quill ? root.__quill : null;
        },

        getHtml: function (containerId) {
            var quill = this.getQuill(containerId);
            return quill ? quill.root.innerHTML : '';
        },

        trimTrailingEmptyParagraph: function (html) {
            var current = (html || '').trim();
            if (current.endsWith('<p><br></p>')) {
                return current.slice(0, -11);
            }
            if (current.endsWith('<p></p>')) {
                return current.slice(0, -7);
            }
            return current;
        },

        wrapAsQuoteBlock: function (html) {
            var inner = (html || '').trim();
            if (!inner) {
                return '';
            }
            if (inner.indexOf('<blockquote') >= 0) {
                return inner;
            }
            return '<blockquote class="rich-text-quote">' + inner + '</blockquote>';
        },

        clearActiveFormats: function (quill) {
            ['blockquote', 'bold', 'italic', 'underline', 'strike', 'code', 'header', 'list'].forEach(function (name) {
                quill.format(name, false, 'user');
            });
        },

        placeCursorAfterQuote: function (quill) {
            if (!quill) {
                return;
            }

            var index = Math.max(0, quill.getLength() - 1);
            quill.insertText(index, '\n', 'user');
            index = quill.getLength() - 1;
            quill.setSelection(index, 0, 'user');
            this.clearActiveFormats(quill);
            quill.focus();
        },

        placeCursorAtEndDomOnly: function (quill) {
            if (!quill) {
                return;
            }

            var lastParagraph = quill.root.querySelector('p:last-of-type');
            if (lastParagraph) {
                var range = document.createRange();
                range.selectNodeContents(lastParagraph);
                range.collapse(false);
                var selection = window.getSelection();
                if (selection) {
                    selection.removeAllRanges();
                    selection.addRange(range);
                }
            }

            quill.focus();
        },

        placeCursorAtEnd: function (quill) {
            if (!quill) {
                return;
            }

            var paragraphs = quill.root.querySelectorAll('p');
            var lastParagraph = paragraphs.length > 0 ? paragraphs[paragraphs.length - 1] : null;
            if (lastParagraph) {
                var range = document.createRange();
                range.selectNodeContents(lastParagraph);
                range.collapse(false);
                var selection = window.getSelection();
                if (selection) {
                    selection.removeAllRanges();
                    selection.addRange(range);
                }
            }

            var index = Math.max(0, quill.getLength() - 1);
            quill.setSelection(index, 0, 'user');
            this.clearActiveFormats(quill);
            quill.focus();
        },

        applyQuoteToRange: function (quill, startLine, lineCount) {
            if (!quill || lineCount <= 0) {
                return;
            }

            quill.formatLine(startLine, lineCount, 'blockquote', true, 'user');
            var blockquotes = quill.root.querySelectorAll('blockquote');
            if (blockquotes.length > 0) {
                blockquotes[blockquotes.length - 1].classList.add('rich-text-quote');
            }
            this.placeCursorAfterQuote(quill);
        },

        loadPrebuiltHtml: function (quill, html) {
            quill.root.innerHTML = html || '';
            quill.root.querySelectorAll('blockquote').forEach(function (node) {
                node.classList.add('rich-text-quote');
            });

            var expectedQuotes = quill.root.querySelectorAll('blockquote').length;
            var synced = false;

            try {
                var delta = quill.clipboard.convert(quill.root.innerHTML);
                quill.setContents(delta, 'silent');
                var actualQuotes = quill.root.querySelectorAll('blockquote').length;
                synced = actualQuotes >= expectedQuotes;
                if (synced) {
                    quill.root.querySelectorAll('blockquote').forEach(function (node) {
                        node.classList.add('rich-text-quote');
                    });
                } else {
                    quill.root.innerHTML = html || '';
                    quill.root.querySelectorAll('blockquote').forEach(function (node) {
                        node.classList.add('rich-text-quote');
                    });
                }
            } catch (e) {
                // Keep DOM HTML if Quill cannot parse merged blockquotes.
            }

            if (synced) {
                var index = Math.max(0, quill.getLength() - 1);
                quill.setSelection(index, 0, 'silent');
                this.clearActiveFormats(quill);
            } else {
                this.placeCursorAtEndDomOnly(quill);
            }

            quill.focus();
        },

        // Paste plain HTML, then apply blockquote like the toolbar button.
        loadRollContent: function (containerId, html, applyQuote, append) {
            var quill = this.getQuill(containerId);
            if (!quill || !html) {
                return;
            }

            if (!applyQuote && html.indexOf('<blockquote') >= 0) {
                this.loadPrebuiltHtml(quill, html);
                return;
            }

            var delta = quill.clipboard.convert(html);
            quill.setContents(delta, 'user');
            if (applyQuote) {
                var lineCount = Math.max(1, quill.getLines().length);
                this.applyQuoteToRange(quill, 0, lineCount);
            }
        }
    };
})();
