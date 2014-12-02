// Helper function that creates emmet range object
function createRange(start, end) {
    return emmet.require('assets/range').create(start, end - start);
}

// Global shortcut function to execute emmet action
function actionExpandAbbreviation() {
    return emmet.run('expand_abbreviation', editorProxy);
}

// Global shortcut function to execute emmet action
function actionWrapWithAbbreviation(abbr) {
    return emmet.run('wrap_with_abbreviation', editorProxy, abbr);
}

// Global shortcut function to execute emmet action
function actionToggleComment() {
    return emmet.run('toggle_comment', editorProxy);
}

// Global shortcut function to execute emmet action
function actionRemoveTag() {
    return emmet.run('remove_tag', editorProxy);
}

// Global shortcut function to execute emmet action
function actionMergeLines() {
    return emmet.run('merge_lines', editorProxy);
}