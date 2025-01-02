function fillAlertDiv() {
        const errorMessageDiv = document.getElementById('error-message');
        errorMessageDiv.className = 'alert alert-danger mt-4';
        errorMessageDiv.role = 'alert';
        errorMessageDiv.textContent = 'The specified todo item does not exist.';
}

async function submitForm(oFormElement) {
        const formData = new FormData(oFormElement);
        try {
            const todoId = document.getElementById('todo-id').value;
            const response = await fetch(`/todos/${todoId}`, {
                method: 'PUT',
                body: formData
            });
            if (response.ok) {
                alert('Todo item updated successfully.\n'+ 'Result: ' + response.status + ' ' +
                    response.statusText);
                window.location.href = `/update-todo.html?id=${todoId}`;
            }
        } catch (error) {
            console.error('Error:', error);
        }
}

function fillFormForUpdate(todo) {        
    const idInput = document.getElementById('todo-id');
    idInput.value = todo.id;
    
    const nameInput = document.getElementById('name-input-id');
    nameInput.value = todo.name;

    const visibilityInput = document.getElementById('visibility-input-id');
    visibilityInput.value = todo.visibility === 0 ? 'Public':'Private';

    //const attachmentInput = document.getElementById('attachment-input-id');
    // onsubmit="submitForm(this);return false;" nel form rende inutile questo codice commentato
    //const form = document.getElementById('update-form-id');
    // form.addEventListener('submit', function (event) {
    //     event.preventDefault();
    //     submitForm(form);
    // });
}
            
document.addEventListener("DOMContentLoaded", function () {
    const urlParams = new URLSearchParams(window.location.search);
    const todoId = urlParams.get('id');

    if (todoId) {
        fetch(`/todos/${todoId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(todo => {
                fillFormForUpdate(todo)
            })
            .catch(error => {
                fillAlertDiv();
            });
    } else {
        fillAlertDiv();
    }
});