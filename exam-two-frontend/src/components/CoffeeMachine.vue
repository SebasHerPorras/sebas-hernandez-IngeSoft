<template>
  <div class="coffee-machine">
    <h1>☕ Coffee Machine</h1>

    <!-- Estado de carga -->
    <div v-if="loading" class="loading">
      <p>Loading...</p>
    </div>

    <!-- Mensaje de error -->
    <div v-if="error" class="error-message">
      <p>❌ {{ error }}</p>
      <button @click="loadData">Retry</button>
    </div>

    <!-- Contenido principal -->
    <div v-if="!loading && !error" class="content">
      
      <!-- Inventario y Precios -->
      <div class="info-section">
        <div class="card">
          <h2>📦 Available Coffee</h2>
          <div v-if="inventory" class="inventory-list">
            <div v-for="(quantity, coffee) in inventory" :key="coffee" class="inventory-item">
              <span class="coffee-name">{{ coffee }}</span>
              <span class="coffee-quantity">{{ quantity }} available</span>
              <span class="coffee-price">₡{{ formatPrice(prices[coffee]) }}</span>
            </div>
          </div>
        </div>

        <div class="card">
          <h2>💰 Available Change</h2>
          <div v-if="availableChange" class="change-list">
            <div v-for="(quantity, coin) in availableChange" :key="coin" class="change-item">
              <span>₡{{ coin }} x {{ quantity }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Formulario de Orden -->
      <div class="order-section">
        <h2>🛒 Place Your Order</h2>
        
        <div class="order-form">
          <div v-for="(quantity, coffee) in inventory" :key="coffee" class="order-item">
            <label>{{ coffee }} (₡{{ formatPrice(prices[coffee]) }})</label>
            <input 
              type="number" 
              v-model.number="orderForm[coffee]" 
              min="0" 
              :max="quantity"
              placeholder="0"
            />
          </div>

          <div class="payment-section">
            <label>💵 Payment Amount (colones)</label>
            <input 
              type="number" 
              v-model.number="paymentAmount" 
              min="0"
              placeholder="Enter amount"
            />
          </div>

          <div class="order-summary">
            <h3>Order Summary</h3>
            <p><strong>Total Cost:</strong> ₡{{ formatPrice(calculateTotal()) }}</p>
            <p><strong>Payment:</strong> ₡{{ formatPrice(paymentAmount) }}</p>
            <p><strong>Change:</strong> ₡{{ formatPrice(paymentAmount - calculateTotal()) }}</p>
          </div>

          <button 
            @click="submitOrder" 
            :disabled="!canSubmitOrder"
            class="submit-button"
          >
            Place Order
          </button>
        </div>
      </div>

      <!-- Resultado de la orden -->
      <div v-if="orderResult" class="order-result" :class="orderResult.success ? 'success' : 'error'">
        <h3>{{ orderResult.success ? '✅ Success' : '❌ Error' }}</h3>
        <p>{{ orderResult.message }}</p>
        <div v-if="orderResult.success && orderResult.changeBreakdown" class="change-breakdown">
          <h4>Change Breakdown:</h4>
          <ul>
            <li v-for="(quantity, coin) in orderResult.changeBreakdown" :key="coin">
              {{ quantity }} coin(s) of ₡{{ coin }}
            </li>
          </ul>
        </div>
        <button @click="resetOrder">New Order</button>
      </div>
    </div>
  </div>
</template>

<script>
import { getInventory, getPrices, getAvailableChange, placeOrder } from '@/services/coffeeService';

export default {
  name: 'CoffeeMachine',
  data() {
    return {
      loading: true,
      error: null,
      inventory: null,
      prices: null,
      availableChange: null,
      orderForm: {},
      paymentAmount: 0,
      orderResult: null,
    };
  },
  computed: {
    canSubmitOrder() {
      const hasItems = Object.values(this.orderForm).some(qty => qty > 0);
      const total = this.calculateTotal();
      return hasItems && this.paymentAmount >= total;
    }
  },
  mounted() {
    this.loadData();
  },
  methods: {
    async loadData() {
      this.loading = true;
      this.error = null;

      try {
        const [inventoryRes, pricesRes, changeRes] = await Promise.all([
          getInventory(),
          getPrices(),
          getAvailableChange()
        ]);

        if (!inventoryRes.success) throw new Error(inventoryRes.error);
        if (!pricesRes.success) throw new Error(pricesRes.error);
        if (!changeRes.success) throw new Error(changeRes.error);

        this.inventory = inventoryRes.data;
        this.prices = pricesRes.data;
        this.availableChange = changeRes.data;

        // Initialize order form
        this.orderForm = Object.keys(this.inventory).reduce((acc, coffee) => {
          acc[coffee] = 0;
          return acc;
        }, {});

      } catch (err) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },
    calculateTotal() {
      if (!this.prices) return 0;
      return Object.entries(this.orderForm).reduce((total, [coffee, quantity]) => {
        return total + (this.prices[coffee] || 0) * quantity;
      }, 0);
    },
    formatPrice(price) {
      return (price / 100).toFixed(2);
    },
    async submitOrder() {
      // Filtrar solo items con cantidad > 0
      const order = Object.entries(this.orderForm)
        .filter(([_, quantity]) => quantity > 0)
        .reduce((acc, [coffee, quantity]) => {
          acc[coffee] = quantity;
          return acc;
        }, {});

      const orderData = {
        order: order,
        payment: {
          totalAmount: this.paymentAmount,
          coins: [],
          bills: []
        }
      };

      const result = await placeOrder(orderData);
      
      if (result.success) {
        this.orderResult = result.data;
        // Recargar datos para actualizar inventario
        setTimeout(() => this.loadData(), 2000);
      } else {
        this.orderResult = {
          success: false,
          message: result.error
        };
      }
    },
    resetOrder() {
      this.orderResult = null;
      this.orderForm = Object.keys(this.inventory).reduce((acc, coffee) => {
        acc[coffee] = 0;
        return acc;
      }, {});
      this.paymentAmount = 0;
      this.loadData();
    }
  }
};
</script>

<style scoped>
.coffee-machine {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
  font-family: 'Arial', sans-serif;
}

h1 {
  text-align: center;
  color: #6f4e37;
  margin-bottom: 30px;
}

.loading, .error-message {
  text-align: center;
  padding: 40px;
}

.error-message {
  background-color: #fee;
  border: 1px solid #fcc;
  border-radius: 8px;
  color: #c33;
}

.error-message button {
  margin-top: 10px;
  padding: 10px 20px;
  background-color: #c33;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.info-section {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
  margin-bottom: 30px;
}

.card {
  background: white;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.card h2 {
  color: #6f4e37;
  margin-bottom: 15px;
}

.inventory-item, .change-item {
  display: flex;
  justify-content: space-between;
  padding: 10px;
  border-bottom: 1px solid #eee;
}

.inventory-item {
  gap: 10px;
}

.coffee-name {
  font-weight: bold;
  flex: 1;
}

.coffee-quantity {
  color: #666;
  flex: 1;
}

.coffee-price {
  color: #2c5f2d;
  font-weight: bold;
}

.order-section {
  background: white;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  margin-bottom: 20px;
}

.order-form {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.order-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.order-item label {
  font-weight: bold;
  flex: 1;
}

.order-item input {
  width: 100px;
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.payment-section {
  margin-top: 20px;
  padding-top: 20px;
  border-top: 2px solid #eee;
}

.payment-section label {
  display: block;
  font-weight: bold;
  margin-bottom: 10px;
}

.payment-section input {
  width: 100%;
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 16px;
}

.order-summary {
  background: #f9f9f9;
  padding: 15px;
  border-radius: 4px;
  margin-top: 15px;
}

.order-summary h3 {
  margin-top: 0;
  color: #6f4e37;
}

.order-summary p {
  margin: 5px 0;
}

.submit-button {
  padding: 15px;
  background-color: #6f4e37;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 16px;
  font-weight: bold;
  cursor: pointer;
  transition: background-color 0.3s;
}

.submit-button:hover:not(:disabled) {
  background-color: #5a3d2b;
}

.submit-button:disabled {
  background-color: #ccc;
  cursor: not-allowed;
}

.order-result {
  background: white;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  margin-top: 20px;
}

.order-result.success {
  border-left: 4px solid #2c5f2d;
}

.order-result.error {
  border-left: 4px solid #c33;
}

.change-breakdown {
  margin-top: 15px;
  padding: 15px;
  background: #f9f9f9;
  border-radius: 4px;
}

.change-breakdown ul {
  list-style: none;
  padding: 0;
}

.change-breakdown li {
  padding: 5px 0;
}

.order-result button {
  margin-top: 15px;
  padding: 10px 20px;
  background-color: #6f4e37;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

@media (max-width: 768px) {
  .info-section {
    grid-template-columns: 1fr;
  }
}
</style>
