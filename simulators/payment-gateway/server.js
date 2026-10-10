const express = require('express');
const app = express();
app.use(express.json());

app.post('/api/payments', (req, res) => {
    const { orderId, amount } = req.body;

    // Simulamos un retraso de red
    setTimeout(() => {
        // Lógica de simulación simple: si el monto es mayor a 5000, falla
        if (amount > 5000) {
            return res.status(400).json({ status: 'REJECTED', reason: 'Insufficient funds' });
        }

        res.json({
            transactionId: `TXN-${Math.floor(Math.random() * 1000000)}`,
            status: 'APPROVED',
            processedAt: new Date().toISOString()
        });
    }, 1000);
});

const PORT = 4001;
app.listen(PORT, () => console.log(`Payment Gateway Simulator running on port ${PORT}`));