// document ready
$(document).ready(function () {
    // prevent form submit due to an enter key press.
    $(window).keydown(function (event) {
        if (event.keyCode == 13) {
            event.preventDefault();
            return false;
        }
    });
});
// Checks if a string is not null, undefined, and empty
function checkStr(str) {
    return (str !== null && str !== undefined && str.trim() !== "")
}
// Execute the regular expression to parse the card information
function parseCard(cardRaw, regEx, groupInds) {
    //decode u-card
    try {

        if (!checkStr(cardRaw) && !checkStr(regEx) && groupInds !== null && groupInds !== undefined)
            return null;
        cardRaw = cardRaw.trim();

        // create and execute regex
        var regEx = new RegExp(regEx);
        var m = regEx.exec(cardRaw);

        if (m === null || m === undefined || m.length <= 0)
            return null;

        var entryInfo = {
            "firstName": "",
            "lastName": "",
            "middleName": "",
            "studentId": "",
            "email": ""
        };

        // parse regex for data at indices as provided by user
        var datas = ["firstName", "lastName", "middleName", "studentId", "email"];
        var index, i;
        for (index = 0; index < datas.length; index++) {
            var indexName = datas[index];
            if (checkStr(groupInds[indexName])) {
                var indexValues = groupInds[indexName].split(",");
                if (indexValues !== null && indexValues.length > 0) {
                    for (i = 0; i < indexValues.length; i++) {
                        var ind = parseInt(indexValues[i]);
                        if (!isNaN(ind) && ind >= 1 && ind < (m.length - 1)) {
                            if (checkStr(m[ind])) {
                                entryInfo[indexName] = m[ind].trim().toLowerCase();
                                break;
                            }
                        }
                    }
                }
            }
        }            

        return entryInfo;
    }
    catch (err) {
        alert(err.message);
        return null;
    }
}
