// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// Sử dụng sự kiện DOMContentLoaded để chạy mã sau khi trang đã tải hoàn toàn
document.addEventListener("DOMContentLoaded", function () {
    // Chọn phần tử loader và ẩn nó sau 1 giây
    setTimeout(function () {
        document.querySelector('.loader').style.display = 'none';
    }, 1000); // 1 giây
});