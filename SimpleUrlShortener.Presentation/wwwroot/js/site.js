// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
const inputText = document.querySelector(".input-text:disabled");
const copyButton = document.querySelector("button.button");
copyButton.addEventListener('click', () => navigator.clipboard.writeText(inputText.value));