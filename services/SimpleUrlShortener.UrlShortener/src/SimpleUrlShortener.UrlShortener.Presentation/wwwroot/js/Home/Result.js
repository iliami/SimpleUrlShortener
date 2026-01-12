const inputText = document.querySelector(".input-text:disabled");
const copyButton = document.querySelector("button.button");
copyButton.addEventListener('click', () => 
    navigator.clipboard.writeText(inputText.value)
        .catch(() => alert("Copying to clipboard is not allowed!")));

document.addEventListener("DOMContentLoaded", () => {
    const originalUrl = localStorage.getItem(lastOriginalURLKey);
    const shortenedUrl = inputText.value;
    const urlPairs = JSON.parse(localStorage.getItem(originalAndShortenedURLsKey));
    

    if (urlPairs) {
        const pair = new UrlPair(urlPairs.length, originalUrl, shortenedUrl);
        urlPairs.push(pair);
        localStorage.setItem(originalAndShortenedURLsKey, JSON.stringify(urlPairs));
    }
    else {
        const pair = new UrlPair(0, originalUrl, shortenedUrl);
        localStorage.setItem(originalAndShortenedURLsKey, JSON.stringify([pair]));
    }
})