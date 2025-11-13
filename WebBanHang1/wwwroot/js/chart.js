window.renderRevenueChart = (revenueData) => {
    const ctx = document.getElementById('revenueChart').getContext('2d');
    const labels = revenueData.map(data => `Day ${data.day}`);
    const data = revenueData.map(data => data.totalRevenue);

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Doanh Thu',
                data: data,
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
};
