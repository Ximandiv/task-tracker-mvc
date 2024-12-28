const editContainer = document.getElementById("edit-form");

document.getElementById("Remove-Btn").addEventListener("click", event => confirm(event));

function confirm(event) {
    event.preventDefault();

    const removeBtn = document.getElementById("Remove-Btn");

    const container = document.getElementById("Remove-Section");

    const sureTxt = document.createElement("p");
    sureTxt.textContent = "Are you sure you want to delete this task?";

    const btnContainer = document.createElement("div");
    btnContainer.classList.add("d-flex", "justify-content-evenly");

    const sureBtn = document.createElement("button");
    sureBtn.type = "submit";
    sureBtn.classList.add("btn", "btn-success", "btn-sm", "btn-warn");
    sureBtn.textContent = "Yes";

    const cancelBtn = document.createElement("button");
    cancelBtn.type = "button";
    cancelBtn.classList.add("btn", "btn-sm", "btn-cancel");
    cancelBtn.textContent = "No";

    const taskId = document.getElementById("Edit-Btn").dataset.id;

    const form = document.getElementById("remove-form");
    form.classList.remove("w-50");
    form.classList.add("w-100");

    removeBtn.removeEventListener("click", (event) => confirm(event, container));
    removeBtn.remove();
    editContainer.style.display = "none";

    btnContainer.appendChild(sureBtn);
    btnContainer.appendChild(cancelBtn);
    container.appendChild(sureTxt);
    container.appendChild(btnContainer);

    cancelBtn.addEventListener("click", (event) => abort(event, container, cancelBtn, taskId));
}

function abort(event, container, cancelBtn, taskId) {
    event.preventDefault();

    container.replaceChildren();

    const form = document.getElementById("remove-form");
    form.classList.remove("w-100");
    form.classList.add("w-50");

    const removeSection = document.getElementById("Remove-Section");

    const removeBtn = document.createElement("button");
    removeBtn.type = "button";
    removeBtn.classList.add("btn", "btn-remove", "btn-sm");
    removeBtn.id = "Remove-Btn";
    removeBtn.textContent = "Remove";

    removeSection.appendChild(removeBtn);

    editContainer.style = "";

    removeBtn.addEventListener("click", event => confirm(event));

    cancelBtn.removeEventListener("click", event => abort(event));
}