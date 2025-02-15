const indexInputForm = document.querySelector('.input-form');
const inputText = document.querySelector(".input-text");

indexInputForm.addEventListener("submit", OnSubmit);

function OnSubmit(event) {
    event.preventDefault();
    const isUrl = inputText.value
        .match(/^(https?:\/\/)?(www\.)?([a-zA-Z0-9\-]+\.)+[a-zA-Z0-9]+([-a-zA-Z0-9()@:%_+.~#?&\[\]/=]*)$/g);

    if (!isUrl) {
        inputText.classList.add('invalid');
        setTimeout(() => inputText.classList.remove('invalid'), 2500);

        return false;
    }

    localStorage.setItem(lastOriginalURLKey, inputText.value);
    indexInputForm.submit();
    return true;
}