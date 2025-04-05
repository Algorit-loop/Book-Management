// userManagement.js - SignalR client functionality for real-time updates

// Create connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/userHub")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 20000])
    .build();

// Start connection
async function startConnection() {
    try {
        await connection.start();
        console.log("SignalR connected");
        // Trigger connected event
        connection.invoke("SendUserStatusChange", "", "connected")
            .catch(err => console.error("Error invoking connected event:", err));
            
        // Update UI directly too
        const connectedEl = document.getElementById('signalrConnected');
        const disconnectedEl = document.getElementById('signalrDisconnected');
        if (connectedEl && disconnectedEl) {
            connectedEl.style.display = 'inline';
            disconnectedEl.style.display = 'none';
        }
    } catch (err) {
        console.error("SignalR connection error: " + err);
        setTimeout(startConnection, 5000);
    }
}

// Handle user list updates
connection.on("ReceiveUserListUpdate", (users) => {
    console.log("Received user list update:", users);
    updateUserTable(users);
});

// Handle user status change notifications
connection.on("ReceiveUserStatusChange", (username, action) => {
    console.log(`User ${username} was ${action}`);
    if (action !== "connected") {
        showNotification(username, action);
    } else {
        // Just update connection status
        const connectedEl = document.getElementById('signalrConnected');
        const disconnectedEl = document.getElementById('signalrDisconnected');
        if (connectedEl && disconnectedEl) {
            connectedEl.style.display = 'inline';
            disconnectedEl.style.display = 'none';
        }
    }
});

// Update the user table with new data
function updateUserTable(users) {
    const tableBody = document.querySelector("#userTable tbody");
    if (!tableBody) return;

    // Clear existing rows
    tableBody.innerHTML = "";

    // Add updated users
    users.forEach(user => {
        const row = document.createElement("tr");
        row.className = !user.isActive ? "table-secondary" : "";

        // User ID
        const idCell = document.createElement("td");
        idCell.textContent = user.id;
        row.appendChild(idCell);

        // Username
        const usernameCell = document.createElement("td");
        usernameCell.textContent = user.username;
        row.appendChild(usernameCell);

        // Role
        const roleCell = document.createElement("td");
        const roleBadge = document.createElement("span");
        roleBadge.className = `badge bg-${user.role === "Admin" ? "danger" : "primary"}`;
        roleBadge.textContent = user.role;
        roleCell.appendChild(roleBadge);
        row.appendChild(roleCell);

        // Status
        const statusCell = document.createElement("td");
        const statusBadge = document.createElement("span");
        statusBadge.className = `badge bg-${user.isActive ? "success" : "secondary"}`;
        statusBadge.textContent = user.isActive ? "Active" : "Banned";
        statusCell.appendChild(statusBadge);
        row.appendChild(statusCell);

        // Actions
        const actionsCell = document.createElement("td");
        const btnGroup = document.createElement("div");
        btnGroup.className = "btn-group";
        
        if (user.role !== "Admin") {
            if (user.isActive) {
                // Ban button
                const banForm = document.createElement("form");
                banForm.method = "post";
                banForm.action = `?handler=Ban&id=${user.id}`;
                banForm.onsubmit = () => confirm('Are you sure you want to ban this user?');
                
                const banButton = document.createElement("button");
                banButton.type = "submit";
                banButton.className = "btn btn-sm btn-warning me-1";
                banButton.innerHTML = '<i class="bi bi-slash-circle"></i> Ban';
                
                banForm.appendChild(banButton);
                btnGroup.appendChild(banForm);
            } else {
                // Unban button
                const unbanForm = document.createElement("form");
                unbanForm.method = "post";
                unbanForm.action = `?handler=Unban&id=${user.id}`;
                
                const unbanButton = document.createElement("button");
                unbanButton.type = "submit";
                unbanButton.className = "btn btn-sm btn-success me-1";
                unbanButton.innerHTML = '<i class="bi bi-check-circle"></i> Unban';
                
                unbanForm.appendChild(unbanButton);
                btnGroup.appendChild(unbanForm);
            }
            
            // Delete button
            const deleteForm = document.createElement("form");
            deleteForm.method = "post";
            deleteForm.action = `?handler=Delete&id=${user.id}`;
            deleteForm.onsubmit = () => confirm('Are you sure you want to delete this user? This action cannot be undone.');
            
            const deleteButton = document.createElement("button");
            deleteButton.type = "submit";
            deleteButton.className = "btn btn-sm btn-danger";
            deleteButton.innerHTML = '<i class="bi bi-trash"></i> Delete';
            
            deleteForm.appendChild(deleteButton);
            btnGroup.appendChild(deleteForm);
        } else {
            const noActions = document.createElement("span");
            noActions.className = "text-muted";
            noActions.textContent = "No actions available";
            btnGroup.appendChild(noActions);
        }
        
        actionsCell.appendChild(btnGroup);
        row.appendChild(actionsCell);

        tableBody.appendChild(row);
    });
    
    // Add anti-forgery tokens to all forms
    initializeForms();
}

// Show a notification when a user's status changes
function showNotification(username, action) {
    // Check if notification container exists, if not create it
    let notifContainer = document.getElementById("signalr-notifications");
    if (!notifContainer) {
        notifContainer = document.createElement("div");
        notifContainer.id = "signalr-notifications";
        notifContainer.className = "position-fixed bottom-0 end-0 p-3";
        document.body.appendChild(notifContainer);
    }
    
    // Create a new notification
    const notifId = `notif-${Date.now()}`;
    const toast = document.createElement("div");
    toast.className = "toast show";
    toast.id = notifId;
    toast.setAttribute("role", "alert");
    toast.setAttribute("aria-live", "assertive");
    toast.setAttribute("aria-atomic", "true");
    
    // Set color based on action
    let bgColor = "bg-info";
    let icon = "info-circle";
    if (action === "banned") {
        bgColor = "bg-warning";
        icon = "slash-circle";
    } else if (action === "unbanned") {
        bgColor = "bg-success";
        icon = "check-circle";
    } else if (action === "deleted") {
        bgColor = "bg-danger";
        icon = "trash";
    }
    
    toast.innerHTML = `
        <div class="toast-header ${bgColor} text-white">
            <i class="bi bi-${icon} me-2"></i>
            <strong class="me-auto">User Update</strong>
            <small>Just now</small>
            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
        <div class="toast-body">
            User <strong>${username}</strong> was ${action}.
        </div>
    `;
    
    notifContainer.appendChild(toast);
    
    // Remove notification after 5 seconds
    setTimeout(() => {
        const toastEl = document.getElementById(notifId);
        if (toastEl) {
            toastEl.remove();
        }
    }, 5000);
}

// Initialize forms with anti-forgery tokens
function initializeForms() {
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!token) {
        console.warn("Anti-forgery token not found on page");
        return;
    }
    
    const forms = document.querySelectorAll("form");
    forms.forEach(form => {
        if (form.method.toLowerCase() === "post") {
            // Create hidden input for the token if it doesn't exist
            if (!form.querySelector('input[name="__RequestVerificationToken"]')) {
                const tokenInput = document.createElement("input");
                tokenInput.type = "hidden";
                tokenInput.name = "__RequestVerificationToken";
                tokenInput.value = token.value;
                form.appendChild(tokenInput);
            }
        }
    });
}

// Start connection when document is ready
document.addEventListener("DOMContentLoaded", function() {
    startConnection();
    
    // Handle forms when dynamically generated
    const observer = new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            if (mutation.type === 'childList') {
                initializeForms();
            }
        });
    });
    
    observer.observe(document.body, { childList: true, subtree: true });
}); 