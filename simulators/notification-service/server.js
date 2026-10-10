const express = require('express');
const app = express();
app.use(express.json());

app.post('/api/notifications', (req, res) => {
    const { to, subject, body } = req.body;

    // En un sistema real esto enviaría un email. Aquí solo registramos.
    console.log(`[EXTERNAL_EMAIL_SENT] To: ${to} | Subject: ${subject}`);

    res.json({ status: 'DELIVERED', timestamp: new Date().toISOString() });
});

const PORT = 4002;
app.listen(PORT, () => console.log(`Notification Simulator running on port ${PORT}`));