#!/bin/bash
HOST="https://localhost:7151"

echo "=== 1. Admin JWT ==="
ADMIN_TOKEN=$(curl -s -X POST $HOST/api/orders/jwt \
  -H "Content-Type: application/json" \
  -d '{"userName":"admin","password":"Passw0rd"}' | jq -r .token)

echo "Admin Token: $ADMIN_TOKEN"

echo "=== 2. GET Orders (Admin) ==="
curl -s -w "Status: %{http_code}\n" -X GET $HOST/api/orders \
  -H "Authorization: Bearer $ADMIN_TOKEN" | jq .

echo "=== 3. POST Order (Admin) ==="
curl -s -w "Status: %{http_code}\n" -X POST $HOST/api/orders \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"date":"2025-10-29T12:00:00Z","clientId":1,"productId":1,"quantity":2}'
