$(document).ready(function () {
    updateFormat();
    var dt = new Date();
    $(".footer").html("Powered by the U of M IEEE Tech Subcommittee. &copy; " + dt.getFullYear() + " - University of Minnesota IEEE Student Branch");
});
function updateFormat() {

    var iu = $.cookie("image-url");
    if (iu != null && iu != undefined) {
        $.cookie("image-url", iu, { expires: 365, path: "/" });
        $("#logoImage").attr("src", iu);
    }

    var bbc = $.cookie("body-background-color");
    if (bbc != null && bbc != undefined) {
        $.cookie("body-background-color", bbc, { expires: 365, path: "/" });
        $("body").css("background-color", "#" + bbc);
    }

    var bubc = $.cookie("button-background-color");
    if (bubc != null && bubc != undefined) {
        $.cookie("button-background-color", bubc, { expires: 365, path: "/" });
        $("button").css("background-color", "#" + bubc);
        $("input[type='submit'][class*='form-control']").css("background-color", "#" + bubc);
    }

    var bc = $.cookie("body-color"); // text color
    if (bc != null && bc != undefined) {
        $.cookie("body-color", bc, { expires: 365, path: "/" });
        $("body").css("color", "#" + bc);
        $("button").css("color", "#" + bc);
        $("input[type='submit'][class*='form-control']").css("color", "#" + bc);
        $("[class*='boxed-section']").css("border", "1px dashed #" + bc);
    }

    var ts = $.cookie("theme-shade");
    if (ts != null && ts != undefined && ts === "light") {
        $.cookie("theme-shade", ts, { expires: 365, path: "/" });
        $("img[class*='shaded']").each(function () {
            $(this).attr("src", $(this).attr("src").replace("dark", "light"));
        });
    }
    else if (ts != null && ts != undefined && ts === "dark") {
        $.cookie("theme-shade", ts, { expires: 365, path: "/" });
        $("img[class*='shaded']").each(function () {
            $(this).attr("src", $(this).attr("src").replace("light", "dark"));
        });
    }

    var ht = $.cookie("header-text");
    if (ht != null && ht != undefined) {
        $.cookie("header-text", ht, { expires: 365, path: "/" });
        $("#topHeader").html(ht);
    }

    var us = $.cookie("use-swipe");
    if (us != null && us != undefined && us === "true") {
        $.cookie("use-swipe", us, { expires: 365, path: "/" });
        $("#swipe-section").attr("style", "display: inherit;");
    }
    else {
        $("#swipe-section").attr("style", "display: none;");
    }

    var sel = $.cookie("card-regex");
    if (sel != null && sel != undefined)
        $.cookie("card-regex", sel, { expires: 365, path: "/" });
}