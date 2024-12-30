document.addEventListener("DOMContentLoaded", () => {
    const tasks = document.querySelectorAll(".task");

    const taskGroups = Array.from(tasks).map((task, index) => {
        return {
            taskIndex: index,
            taskElement: task,
            editBtn: task.querySelector(".edit-btn"),
            removeBtn: task.querySelector(".remove-btn"),
            form: task.querySelector(".remove-form")
        }
    });

    taskGroups.forEach((taskGroup) => {
        const { removeBtn } = taskGroup;

        removeBtn.addEventListener("click", (event) => confirm(event, taskGroup));
    });
})

function confirm(event, taskGroup) {
    event.preventDefault();

    const { taskElement, editBtn, form } = taskGroup;

    editBtn.parentNode.style.display = "none";
    form.style.display = "none";

    const controlsContainer = taskElement.querySelector(".task-controls");

    const btnContainer = document.createElement("div");
    btnContainer.id = "btn-confirmation";
    btnContainer.classList.add("d-flex", "flex-column", "align-items-center", "gap-3");

    const confirmationText = document.createElement("p");
    confirmationText.id = "confirmation";
    confirmationText.textContent = "Are you sure you want to delete this task?";
    confirmationText.classList.add("text-center", "mb-2");

    const buttonRow = document.createElement("div");
    buttonRow.classList.add("d-flex", "justify-content-between", "w-100", "gap-2");

    const confirmBtn = document.createElement("button");
    confirmBtn.type = "submit";
    confirmBtn.classList.add("btn", "btn-success", "btn-sm", "w-50");
    confirmBtn.textContent = "Yes";

    const cancelBtn = document.createElement("button");
    cancelBtn.type = "button";
    cancelBtn.classList.add("btn", "btn-sm", "btn-cancel", "w-50");
    cancelBtn.textContent = "No";

    buttonRow.appendChild(confirmBtn);
    buttonRow.appendChild(cancelBtn);

    btnContainer.appendChild(confirmationText);
    btnContainer.appendChild(buttonRow);

    controlsContainer.appendChild(btnContainer);

    confirmBtn.addEventListener("click", () => form.submit());
    cancelBtn.addEventListener("click", () => abort(taskGroup, controlsContainer, cancelBtn));
}

function abort(taskGroup, controlsContainer, cancelBtn) {
    const { editBtn, form } = taskGroup;

    cancelBtn.removeEventListener("click", () => abort(taskGroup, controlsContainer, cancelBtn));

    controlsContainer.querySelector("#confirmation").remove();
    controlsContainer.querySelector("#btn-confirmation").remove();

    editBtn.parentNode.style.display = "block";
    form.style.display = "block";
}