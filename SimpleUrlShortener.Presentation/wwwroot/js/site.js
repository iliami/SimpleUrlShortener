const originalAndShortenedURLsKey = "OriginalAndShortened_URLs";
const lastOriginalURLKey = "LastOriginal_URL";

class UrlPair {
    constructor(id, originalUrl, shortenedUrl) {
        this.id = id;
        this.originalUrl = originalUrl;
        this.shortenedUrl = shortenedUrl;
        this.createdAt = Date.now();
    }
}


const inputForm = document.querySelector(".input-form");
window.addEventListener("load", AddStoredUrls)

const table = document.createElement('table');

function AddStoredUrls() {
    cleanUrlPairs();
    const urlPairs = JSON.parse(localStorage.getItem(originalAndShortenedURLsKey));
    if (!urlPairs || urlPairs.length === 0) {
        localStorage.setItem(originalAndShortenedURLsKey, JSON.stringify([]));
        return;
    }
    
    
    
    const tableRows = [];
    for (const url of urlPairs) {
        if (!url) continue;
        const tableRow = `<tr id="${url.id}"><td>&#215;</td><td>${url.originalUrl}</td><td>${url.shortenedUrl}</td></tr>`
        tableRows.push(tableRow);
    }

    const tableDiv = document.createElement("div");
    inputForm.parentNode.parentNode.appendChild(tableDiv);
    tableDiv.style.marginTop = "30px";
    tableDiv.appendChild(table);
    table.innerHTML = "<thead><tr><th></th><th>Original URL</th><th>Shortened URL</th></tr></thead>";
    table.firstElementChild.insertAdjacentHTML('afterend', "<tbody>" + tableRows.reverse().join(" ") + "</tbody>");
    for (const tr of table.children.item(1).children) {
        tr.firstChild.addEventListener("click", RemoveStoredUrlPair);
    }
}

function RemoveStoredUrlPair(event) {
    const tableRowId = event.target.parentNode.id;
    const urlPairs = JSON.parse(localStorage.getItem(originalAndShortenedURLsKey))
        .filter((item) => item.id != tableRowId);
    localStorage.setItem(originalAndShortenedURLsKey, JSON.stringify(urlPairs));

    table.children.item(1).removeChild(event.target.parentNode);
}

const minimalDiff = (6 * 24 + 23) * 60 * 60 * 1000;
function cleanUrlPairs() {
    const currentTime = Date.now();
    const urlPairs = JSON.parse(localStorage.getItem(originalAndShortenedURLsKey)).filter(item => item.createdAt - currentTime < minimalDiff);
    localStorage.setItem(originalAndShortenedURLsKey, JSON.stringify(urlPairs));
}