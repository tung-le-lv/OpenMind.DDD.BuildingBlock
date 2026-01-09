// Initialize databases for Order and Payment services
db = db.getSiblingDB('OrderDb');
db.createCollection('orders');
db.orders.createIndex({ "CustomerId": 1 });
db.orders.createIndex({ "Status": 1 });
db.orders.createIndex({ "CreatedAt": -1 });

db = db.getSiblingDB('PaymentDb');
db.createCollection('payments');
db.payments.createIndex({ "OrderId": 1 });
db.payments.createIndex({ "CustomerId": 1 });
db.payments.createIndex({ "Status": 1 });
db.payments.createIndex({ "CreatedAt": -1 });

print('MongoDB initialized with OrderDb and PaymentDb databases');
