export function debounce(fn, delay) {
    var timeoutID = null
    return function () {
        clearTimeout(timeoutID)
        var args = arguments
        var that = this
        timeoutID = setTimeout(function () {
            fn.apply(that, args)
        }, delay)
    }
}

export function getInitials(profile) {
    if (!profile.name) return "";
    var splited = profile.name.split(" ");
    if (splited.length > 1) return splited[0][0] + splited[1][0];
    return splited[0][0];
}