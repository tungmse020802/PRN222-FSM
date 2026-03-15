(() => {
    const sidebarStorageKey = "sms.sidebar.collapsed";
    const groupStoragePrefix = "sms.sidebar.group.";
    const sidebarToggle = document.querySelector("[data-sidebar-toggle]");
    const groupToggles = document.querySelectorAll("[data-sidebar-group-toggle]");

    const applySidebarState = isCollapsed => {
        document.body.classList.toggle("sidebar-collapsed", isCollapsed);
        if (sidebarToggle) {
            sidebarToggle.setAttribute("aria-expanded", (!isCollapsed).toString());
            sidebarToggle.textContent = isCollapsed ? "Menu" : "Hide menu";
        }
    };

    const applyGroupState = (group, isOpen) => {
        group.classList.toggle("is-open", isOpen);
        const toggle = group.querySelector("[data-sidebar-group-toggle]");
        toggle?.setAttribute("aria-expanded", isOpen.toString());
    };

    const savedSidebarState = window.localStorage.getItem(sidebarStorageKey);
    applySidebarState(savedSidebarState === "true");

    sidebarToggle?.addEventListener("click", () => {
        const nextState = !document.body.classList.contains("sidebar-collapsed");
        applySidebarState(nextState);
        window.localStorage.setItem(sidebarStorageKey, nextState.toString());
    });

    groupToggles.forEach(toggle => {
        const group = toggle.closest("[data-sidebar-group]");
        if (!group) {
            return;
        }

        const groupKey = group.getAttribute("data-sidebar-group");
        const savedState = groupKey ? window.localStorage.getItem(`${groupStoragePrefix}${groupKey}`) : null;
        if (savedState !== null) {
            applyGroupState(group, savedState === "true");
        }
        else {
            applyGroupState(group, group.classList.contains("is-open"));
        }

        toggle.addEventListener("click", () => {
            const nextState = !group.classList.contains("is-open");
            applyGroupState(group, nextState);
            if (groupKey) {
                window.localStorage.setItem(`${groupStoragePrefix}${groupKey}`, nextState.toString());
            }
        });
    });
})();

(() => {
    if (document.body?.dataset.liveNotifications !== "true") {
        return;
    }

    if (!window.signalR) {
        return;
    }

    const notificationPages = new Set([
        "/Student/Notifications",
        "/Lecturer/Notifications"
    ]);
    const defaultNotificationsUrl = document.body.dataset.notificationsUrl || "/";
    const toastContainer = document.getElementById("notificationToastContainer");
    let reconnectDelay = 1000;

    const escapeHtml = value => (value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;")
        .replaceAll("'", "&#39;");

    const formatTimestamp = value => {
        if (!value) {
            return "";
        }

        const date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return "";
        }

        return new Intl.DateTimeFormat(undefined, {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit"
        }).format(date);
    };

    const buildToastMarkup = payload => {
        const title = escapeHtml(payload?.title || "New notification");
        const message = escapeHtml(payload?.message || "");
        const type = escapeHtml(payload?.type || "System");
        const url = payload?.url || defaultNotificationsUrl;
        const sentAt = formatTimestamp(payload?.sentAt);
        const icon = payload?.type === "Grade"
            ? "G"
            : payload?.type === "Registration"
                ? "R"
                : payload?.type === "Recommendation"
                    ? "P"
                    : "N";

        return `
            <div class="toast notification-toast" role="alert" aria-live="assertive" aria-atomic="true" data-notification-url="${escapeHtml(url)}">
                <div class="toast-body notification-toast-body">
                    <div class="notification-toast-icon">${icon}</div>
                    <div class="notification-toast-content">
                        <div class="notification-toast-head">
                            <strong class="notification-toast-title">${title}</strong>
                            ${sentAt ? `<small class="notification-toast-time">${sentAt}</small>` : ""}
                        </div>
                        <div class="notification-toast-message">${message}</div>
                        <div class="notification-toast-actions">
                            <span class="notification-toast-badge">${type}</span>
                            <a class="notification-toast-link" href="${escapeHtml(url)}">View details</a>
                        </div>
                    </div>
                    <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;
    };

    const showToast = payload => {
        if (!toastContainer) {
            return;
        }

        toastContainer.insertAdjacentHTML("beforeend", buildToastMarkup(payload));
        const toastElement = toastContainer.lastElementChild;
        if (!toastElement) {
            return;
        }

        toastElement.addEventListener("hidden.bs.toast", () => {
            toastElement.remove();
        });

        bootstrap.Toast.getOrCreateInstance(toastElement, {
            autohide: true,
            delay: 5000
        }).show();
    };

    const refreshNotificationsPage = () => {
        if (!notificationPages.has(window.location.pathname)) {
            return;
        }

        window.setTimeout(() => window.location.reload(), 250);
    };

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveNotification", payload => {
        showToast(payload);
    });

    connection.on("RefreshNotifications", () => {
        refreshNotificationsPage();
    });

    connection.onreconnecting(() => {
        reconnectDelay = 1000;
    });

    const startConnection = async () => {
        try {
            await connection.start();
            reconnectDelay = 1000;
        }
        catch (error) {
            window.setTimeout(startConnection, reconnectDelay);
            reconnectDelay = Math.min(reconnectDelay * 2, 15000);
        }
    };

    void startConnection();
})();
