function fillAlertDiv() {
  const errorMessageDiv = document.getElementById("error-message");
  errorMessageDiv.className = "alert alert-danger mt-4";
  errorMessageDiv.role = "alert";
  errorMessageDiv.textContent = "The specified todo item does not exist.";
}

function fillCardDivForDelete(todo) {
  const cardDiv = document.getElementById("todo-card");
  cardDiv.className = "card mt-4";

  const cardBodyDiv = document.createElement("div");
  cardBodyDiv.className = "card-body";

  const cardTitle = document.createElement("h5");
  cardTitle.className = "card-title";
  cardTitle.id = "todo-name";
  cardTitle.textContent =`Nome: ${todo.name}`;

  const cardSubtitle = document.createElement("p");
  cardSubtitle.className = "card-subtitle mb-4";
  cardSubtitle.id = "todo-id";
  cardSubtitle.textContent = `ID: ${todo.id}`;

  const cardVisibility = document.createElement("p");
  cardVisibility.className = "card-text";
  cardVisibility.id = "todo-visibility";
  cardVisibility.textContent = `Visibility: ${todo.visibility === 0 ? "Public" : "Private"}`;

  const cardAttachment = document.createElement("p");
  cardAttachment.className = "card-text";
  cardAttachment.id = "todo-attachment";
  cardAttachment.textContent = `Attachment: ${todo.attachment}`;

  const deleteButton = document.createElement("input");
  deleteButton.type = "submit";
  deleteButton.id = "delete-button";
  deleteButton.className = "btn btn-danger";
  deleteButton.value = "Delete";
  deleteButton.addEventListener("click", function () {
    const todoId = todo.id;
    fetch(`/todos/${todoId}`, {
      method: "DELETE",
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error("Network response was not ok");
        }
        alert("Todo item deleted successfully.");
        window.location.href = "todos.html";
      })
      .catch((error) => {
        alert("Failed to delete the todo item.");
      });
  });
  cardBodyDiv.appendChild(cardTitle);
  cardBodyDiv.appendChild(cardSubtitle);
  cardBodyDiv.appendChild(cardVisibility);
  cardBodyDiv.appendChild(cardAttachment);
  cardBodyDiv.appendChild(deleteButton);
  cardDiv.appendChild(cardBodyDiv);
}

document.addEventListener("DOMContentLoaded", function () {
  const urlParams = new URLSearchParams(window.location.search);
  const todoId = urlParams.get("id");
  if (todoId) {
    fetch(`/todos/${todoId}`)
      .then((response) => {
        if (!response.ok) {
          throw new Error("Network response was not ok");
        }
        return response.json();
      })
      .then((todo) => {
        fillCardDivForDelete(todo);
      })
      .catch((error) => {
        fillAlertDiv();
      });
  } else {
    fillAlertDiv();
  }
});
