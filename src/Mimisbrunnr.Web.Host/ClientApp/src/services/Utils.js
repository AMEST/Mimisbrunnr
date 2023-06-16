import { BToast } from 'bootstrap-vue'
import router from '@/router'
var bootStrapToaster = null;

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
    return getNameInitials(profile.name);
}

export function getNameInitials(name) {
    if (!name) return "";
    var splited = name.split(" ");
    if (splited.length > 1) return splited[0][0] + splited[1][0];
    return splited[0][0];
}

export function showToast(message, title, variant) {
    if (bootStrapToaster == null)
        bootStrapToaster = new BToast();

    bootStrapToaster.$bvToast.toast(message, {
        title: title,
        variant: variant,
        solid: true
    });
}

export function isImageFile(name) {
    return (
        name.toLowerCase().endsWith(".png") ||
        name.toLowerCase().endsWith(".jpg") ||
        name.toLowerCase().endsWith(".jpeg") ||
        name.toLowerCase().endsWith(".gif") ||
        name.toLowerCase().endsWith(".svg")
    );
}

export function replaceRelativeLinksToRoute(elementId){
    var block = document.getElementById(elementId);
    if(!block) return;

    var links = block.getElementsByTagName("a");
    if(!links) return;

    var linksForReplace = Array.from(links).filter(x =>  x.getAttribute("href").startsWith("/") && !x.getAttribute("href").startsWith("/api/"));
    if(!linksForReplace) return;

    for(let link of linksForReplace){
        link.addEventListener("click", function (e) {
            e.preventDefault(); 
            router.push(e.target.getAttribute("href"));
        })
    }
}