function deleteSelectedServices(ProductId) {
    var apiUrl = '/api/ServicesApi/deleteAll/';

    fetch(apiUrl, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(ProductId),
    })
    .then(response => {
        if (response.ok) {
            return response.json();
        } else {
            throw new Error(response.status);
        }
    })
    .then(result => {
        console.log('Result:', result);

        if (result.message.toLowerCase().includes('successfully')) {
            alert(result.message);
            window.location.reload();
        } else {
            alert('Failed to delete ServiceType: ' + result.message);
            window.location.reload();
        }
    })
    .catch(error => {
        console.error('Error deleting ServiceType:', error);
        alert('Delete Successfull.');
        window.location.reload();
    });
}

document.addEventListener('DOMContentLoaded', function () {
    // First section
    document.getElementById('selectAll').addEventListener('click', function () {
        var checkboxes = document.querySelectorAll('.productCheckbox');
        checkboxes.forEach(function (checkbox) {
            checkbox.checked = this.checked;
        }, this);
    });

    var checkboxes = document.querySelectorAll('.productCheckbox');
    checkboxes.forEach(function (checkbox) {
        checkbox.addEventListener('click', function () {
            var anyChecked = document.querySelectorAll('.productCheckbox:checked').length > 0;
            document.getElementById('deleteSelected').disabled = !anyChecked;
        });
    });

    document.getElementById('deleteSelected').addEventListener('click', function () {
        var selectedProductIds = Array.from(document.querySelectorAll('.productCheckbox:checked')).map(function (checkbox) {
            return checkbox.getAttribute('data-product-id');
        });

        if (selectedProductIds.length > 0) {
            console.log('Selected Product IDs:', selectedProductIds);
            deleteSelectedServices(selectedProductIds);
            window.location.reload();
        } else {
            console.log('No products selected.');
        }
    });

    // Second section
    const searchInput = document.getElementById("searchInput");
    const searchButton = document.getElementById("searchButton");

    searchInput.addEventListener("input", function () {
        if (searchInput.value.trim() === "") {
            searchButton.disabled = true;
        } else {
            searchButton.disabled = false;
        }
    });
});

function navigateTo(action, ServiceId) {
    window.location.href = `/Admin/Services/${action}?ServiceId=${ServiceId}`;
}

function deleteStaff(ServiceId) {
    if (confirm('Are you sure you want to delete this Service?')) {
        navigateTo('Delete', ServiceId);
    }
}
