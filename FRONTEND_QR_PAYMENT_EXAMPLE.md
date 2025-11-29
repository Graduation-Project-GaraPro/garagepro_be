# Frontend QR Payment Integration Example

## Overview
This document provides frontend code examples for integrating the manager QR payment feature.

## TypeScript/React Example

### 1. Payment Service

```typescript
// services/payment-service.ts

export interface ManagerQrPaymentRequest {
  method: 'PayOs';
  description?: string;
}

export interface ManagerQrPaymentResponse {
  message: string;
  paymentId: number;
  orderCode: number;
  checkoutUrl: string;
  qrCodeUrl: string;
}

export interface PaymentStatusResponse {
  orderCode: number;
  status: 'Unpaid' | 'Paid' | 'Cancelled' | 'Failed';
  providerCode?: string;
  providerDesc?: string;
}

export class PaymentService {
  private baseUrl = '/api/Payments';

  async createManagerQrPayment(
    repairOrderId: string,
    request: ManagerQrPaymentRequest
  ): Promise<ManagerQrPaymentResponse> {
    const response = await fetch(
      `${this.baseUrl}/manager-qr-payment/${repairOrderId}`,
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${this.getToken()}`,
        },
        body: JSON.stringify(request),
      }
    );

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to create QR payment');
    }

    return response.json();
  }

  async getPaymentStatus(orderCode: number): Promise<PaymentStatusResponse> {
    const response = await fetch(`${this.baseUrl}/status/${orderCode}`, {
      headers: {
        Authorization: `Bearer ${this.getToken()}`,
      },
    });

    if (!response.ok) {
      throw new Error('Failed to get payment status');
    }

    return response.json();
  }

  private getToken(): string {
    // Get token from your auth system
    return localStorage.getItem('authToken') || '';
  }
}
```

### 2. QR Payment Dialog Component

```typescript
// components/QrPaymentDialog.tsx

import React, { useState, useEffect } from 'react';
import QRCode from 'qrcode';
import { PaymentService } from '../services/payment-service';

interface QrPaymentDialogProps {
  repairOrderId: string;
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const QrPaymentDialog: React.FC<QrPaymentDialogProps> = ({
  repairOrderId,
  isOpen,
  onClose,
  onSuccess,
}) => {
  const [qrCodeDataUrl, setQrCodeDataUrl] = useState<string>('');
  const [orderCode, setOrderCode] = useState<number | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>('');
  const [checkoutUrl, setCheckoutUrl] = useState<string>('');

  const paymentService = new PaymentService();

  // Generate QR code when dialog opens
  useEffect(() => {
    if (isOpen && !qrCodeDataUrl) {
      generateQrCode();
    }
  }, [isOpen]);

  // Poll for payment status
  useEffect(() => {
    if (!orderCode) return;

    const intervalId = setInterval(async () => {
      try {
        const status = await paymentService.getPaymentStatus(orderCode);
        
        if (status.status === 'Paid') {
          clearInterval(intervalId);
          onSuccess();
          onClose();
        } else if (status.status === 'Cancelled' || status.status === 'Failed') {
          clearInterval(intervalId);
          setError('Payment was cancelled or failed');
        }
      } catch (err) {
        console.error('Error checking payment status:', err);
      }
    }, 5000); // Poll every 5 seconds

    return () => clearInterval(intervalId);
  }, [orderCode]);

  const generateQrCode = async () => {
    setLoading(true);
    setError('');

    try {
      const response = await paymentService.createManagerQrPayment(
        repairOrderId,
        {
          method: 'PayOs',
          description: 'Payment for repair services',
        }
      );

      setOrderCode(response.orderCode);
      setCheckoutUrl(response.checkoutUrl);

      // Generate QR code from checkout URL
      const qrDataUrl = await QRCode.toDataURL(response.checkoutUrl, {
        width: 300,
        margin: 2,
      });
      setQrCodeDataUrl(qrDataUrl);
    } catch (err: any) {
      setError(err.message || 'Failed to generate QR code');
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="dialog-overlay">
      <div className="dialog-content">
        <div className="dialog-header">
          <h2>QR Payment</h2>
          <button onClick={onClose} className="close-button">×</button>
        </div>

        <div className="dialog-body">
          {loading && (
            <div className="loading">
              <p>Generating QR code...</p>
            </div>
          )}

          {error && (
            <div className="error-message">
              <p>{error}</p>
              <button onClick={generateQrCode}>Try Again</button>
            </div>
          )}

          {qrCodeDataUrl && !error && (
            <div className="qr-code-container">
              <p className="instruction">
                Ask the customer to scan this QR code with their banking app
              </p>
              
              <img 
                src={qrCodeDataUrl} 
                alt="Payment QR Code" 
                className="qr-code-image"
              />

              <div className="payment-info">
                <p>Order Code: <strong>{orderCode}</strong></p>
                <p className="status-text">Waiting for payment...</p>
              </div>

              <div className="alternative-payment">
                <p>Or customer can click this link:</p>
                <a 
                  href={checkoutUrl} 
                  target="_blank" 
                  rel="noopener noreferrer"
                  className="checkout-link"
                >
                  Open Payment Page
                </a>
              </div>
            </div>
          )}
        </div>

        <div className="dialog-footer">
          <button onClick={onClose} className="cancel-button">
            Close
          </button>
        </div>
      </div>
    </div>
  );
};
```

### 3. Usage in Repair Order Page

```typescript
// pages/RepairOrderDetail.tsx

import React, { useState } from 'react';
import { QrPaymentDialog } from '../components/QrPaymentDialog';

export const RepairOrderDetail: React.FC = () => {
  const [showQrDialog, setShowQrDialog] = useState(false);
  const [repairOrderId, setRepairOrderId] = useState<string>('');

  const handleQrPaymentClick = () => {
    setShowQrDialog(true);
  };

  const handlePaymentSuccess = () => {
    // Refresh repair order data
    loadRepairOrder();
    
    // Show success message
    alert('Payment received successfully!');
  };

  const loadRepairOrder = async () => {
    // Reload repair order data to get updated status
    // Your implementation here
  };

  return (
    <div className="repair-order-detail">
      {/* Your repair order details */}
      
      <div className="payment-section">
        <h3>Payment</h3>
        
        <button 
          onClick={handleQrPaymentClick}
          className="qr-payment-button"
        >
          Generate QR Payment
        </button>
      </div>

      <QrPaymentDialog
        repairOrderId={repairOrderId}
        isOpen={showQrDialog}
        onClose={() => setShowQrDialog(false)}
        onSuccess={handlePaymentSuccess}
      />
    </div>
  );
};
```

## Vue.js Example

### 1. Composable

```typescript
// composables/useQrPayment.ts

import { ref } from 'vue';
import QRCode from 'qrcode';

export function useQrPayment() {
  const qrCodeDataUrl = ref<string>('');
  const orderCode = ref<number | null>(null);
  const loading = ref(false);
  const error = ref<string>('');
  const checkoutUrl = ref<string>('');

  const generateQrCode = async (repairOrderId: string) => {
    loading.value = true;
    error.value = '';

    try {
      const response = await fetch(
        `/api/Payments/manager-qr-payment/${repairOrderId}`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${getToken()}`,
          },
          body: JSON.stringify({
            method: 'PayOs',
            description: 'Payment for repair services',
          }),
        }
      );

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to create QR payment');
      }

      const data = await response.json();
      orderCode.value = data.orderCode;
      checkoutUrl.value = data.checkoutUrl;

      // Generate QR code
      const qrDataUrl = await QRCode.toDataURL(data.checkoutUrl, {
        width: 300,
        margin: 2,
      });
      qrCodeDataUrl.value = qrDataUrl;
    } catch (err: any) {
      error.value = err.message || 'Failed to generate QR code';
    } finally {
      loading.value = false;
    }
  };

  const checkPaymentStatus = async () => {
    if (!orderCode.value) return null;

    try {
      const response = await fetch(
        `/api/Payments/status/${orderCode.value}`,
        {
          headers: {
            Authorization: `Bearer ${getToken()}`,
          },
        }
      );

      if (!response.ok) {
        throw new Error('Failed to get payment status');
      }

      return await response.json();
    } catch (err) {
      console.error('Error checking payment status:', err);
      return null;
    }
  };

  const getToken = () => {
    return localStorage.getItem('authToken') || '';
  };

  return {
    qrCodeDataUrl,
    orderCode,
    loading,
    error,
    checkoutUrl,
    generateQrCode,
    checkPaymentStatus,
  };
}
```

### 2. Component

```vue
<!-- components/QrPaymentDialog.vue -->

<template>
  <div v-if="isOpen" class="dialog-overlay">
    <div class="dialog-content">
      <div class="dialog-header">
        <h2>QR Payment</h2>
        <button @click="$emit('close')" class="close-button">×</button>
      </div>

      <div class="dialog-body">
        <div v-if="loading" class="loading">
          <p>Generating QR code...</p>
        </div>

        <div v-if="error" class="error-message">
          <p>{{ error }}</p>
          <button @click="handleGenerate">Try Again</button>
        </div>

        <div v-if="qrCodeDataUrl && !error" class="qr-code-container">
          <p class="instruction">
            Ask the customer to scan this QR code with their banking app
          </p>
          
          <img 
            :src="qrCodeDataUrl" 
            alt="Payment QR Code" 
            class="qr-code-image"
          />

          <div class="payment-info">
            <p>Order Code: <strong>{{ orderCode }}</strong></p>
            <p class="status-text">Waiting for payment...</p>
          </div>

          <div class="alternative-payment">
            <p>Or customer can click this link:</p>
            <a 
              :href="checkoutUrl" 
              target="_blank" 
              rel="noopener noreferrer"
              class="checkout-link"
            >
              Open Payment Page
            </a>
          </div>
        </div>
      </div>

      <div class="dialog-footer">
        <button @click="$emit('close')" class="cancel-button">
          Close
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { watch, onUnmounted } from 'vue';
import { useQrPayment } from '../composables/useQrPayment';

const props = defineProps<{
  repairOrderId: string;
  isOpen: boolean;
}>();

const emit = defineEmits<{
  close: [];
  success: [];
}>();

const {
  qrCodeDataUrl,
  orderCode,
  loading,
  error,
  checkoutUrl,
  generateQrCode,
  checkPaymentStatus,
} = useQrPayment();

let pollInterval: number | null = null;

watch(() => props.isOpen, (isOpen) => {
  if (isOpen && !qrCodeDataUrl.value) {
    handleGenerate();
  }
});

watch(orderCode, (code) => {
  if (code) {
    startPolling();
  }
});

const handleGenerate = async () => {
  await generateQrCode(props.repairOrderId);
};

const startPolling = () => {
  if (pollInterval) return;

  pollInterval = window.setInterval(async () => {
    const status = await checkPaymentStatus();
    
    if (status?.status === 'Paid') {
      stopPolling();
      emit('success');
      emit('close');
    } else if (status?.status === 'Cancelled' || status?.status === 'Failed') {
      stopPolling();
      error.value = 'Payment was cancelled or failed';
    }
  }, 5000);
};

const stopPolling = () => {
  if (pollInterval) {
    clearInterval(pollInterval);
    pollInterval = null;
  }
};

onUnmounted(() => {
  stopPolling();
});
</script>

<style scoped>
.dialog-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.dialog-content {
  background: white;
  border-radius: 8px;
  max-width: 500px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
}

.qr-code-container {
  text-align: center;
  padding: 20px;
}

.qr-code-image {
  max-width: 300px;
  width: 100%;
  margin: 20px auto;
}

.instruction {
  font-size: 16px;
  color: #666;
  margin-bottom: 10px;
}

.payment-info {
  margin: 20px 0;
}

.status-text {
  color: #ff9800;
  font-weight: 500;
}

.checkout-link {
  color: #2196f3;
  text-decoration: none;
  font-weight: 500;
}

.checkout-link:hover {
  text-decoration: underline;
}
</style>
```

## Plain JavaScript Example

```javascript
// qr-payment.js

class QrPaymentManager {
  constructor(baseUrl = '/api/Payments') {
    this.baseUrl = baseUrl;
    this.pollInterval = null;
  }

  async createQrPayment(repairOrderId) {
    const response = await fetch(
      `${this.baseUrl}/manager-qr-payment/${repairOrderId}`,
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${this.getToken()}`,
        },
        body: JSON.stringify({
          method: 'PayOs',
          description: 'Payment for repair services',
        }),
      }
    );

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to create QR payment');
    }

    return response.json();
  }

  async getPaymentStatus(orderCode) {
    const response = await fetch(`${this.baseUrl}/status/${orderCode}`, {
      headers: {
        Authorization: `Bearer ${this.getToken()}`,
      },
    });

    if (!response.ok) {
      throw new Error('Failed to get payment status');
    }

    return response.json();
  }

  async generateQrCode(checkoutUrl) {
    // Using qrcode library
    return QRCode.toDataURL(checkoutUrl, {
      width: 300,
      margin: 2,
    });
  }

  startPolling(orderCode, onSuccess, onError) {
    this.stopPolling();

    this.pollInterval = setInterval(async () => {
      try {
        const status = await this.getPaymentStatus(orderCode);
        
        if (status.status === 'Paid') {
          this.stopPolling();
          onSuccess(status);
        } else if (status.status === 'Cancelled' || status.status === 'Failed') {
          this.stopPolling();
          onError(new Error('Payment was cancelled or failed'));
        }
      } catch (err) {
        console.error('Error checking payment status:', err);
      }
    }, 5000);
  }

  stopPolling() {
    if (this.pollInterval) {
      clearInterval(this.pollInterval);
      this.pollInterval = null;
    }
  }

  getToken() {
    return localStorage.getItem('authToken') || '';
  }
}

// Usage
const qrPayment = new QrPaymentManager();

async function showQrPaymentDialog(repairOrderId) {
  try {
    // Create payment
    const payment = await qrPayment.createQrPayment(repairOrderId);
    
    // Generate QR code
    const qrCodeDataUrl = await qrPayment.generateQrCode(payment.checkoutUrl);
    
    // Display QR code
    document.getElementById('qr-code-image').src = qrCodeDataUrl;
    document.getElementById('order-code').textContent = payment.orderCode;
    document.getElementById('checkout-link').href = payment.checkoutUrl;
    
    // Show dialog
    document.getElementById('qr-dialog').style.display = 'block';
    
    // Start polling
    qrPayment.startPolling(
      payment.orderCode,
      (status) => {
        alert('Payment received successfully!');
        location.reload();
      },
      (error) => {
        alert(error.message);
      }
    );
  } catch (err) {
    alert(err.message);
  }
}
```

## CSS Styling Example

```css
/* qr-payment-dialog.css */

.dialog-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.dialog-content {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
  max-width: 500px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
}

.dialog-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid #e0e0e0;
}

.dialog-header h2 {
  margin: 0;
  font-size: 24px;
  color: #333;
}

.close-button {
  background: none;
  border: none;
  font-size: 32px;
  color: #999;
  cursor: pointer;
  padding: 0;
  width: 32px;
  height: 32px;
  line-height: 1;
}

.close-button:hover {
  color: #333;
}

.dialog-body {
  padding: 30px;
}

.qr-code-container {
  text-align: center;
}

.instruction {
  font-size: 16px;
  color: #666;
  margin-bottom: 20px;
  line-height: 1.5;
}

.qr-code-image {
  max-width: 300px;
  width: 100%;
  margin: 20px auto;
  display: block;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  padding: 10px;
}

.payment-info {
  margin: 20px 0;
  padding: 15px;
  background: #f5f5f5;
  border-radius: 8px;
}

.payment-info p {
  margin: 8px 0;
  font-size: 14px;
  color: #666;
}

.payment-info strong {
  color: #333;
  font-size: 16px;
}

.status-text {
  color: #ff9800;
  font-weight: 500;
  font-size: 16px;
  margin-top: 10px;
}

.alternative-payment {
  margin-top: 20px;
  padding-top: 20px;
  border-top: 1px solid #e0e0e0;
}

.alternative-payment p {
  font-size: 14px;
  color: #666;
  margin-bottom: 10px;
}

.checkout-link {
  display: inline-block;
  color: #2196f3;
  text-decoration: none;
  font-weight: 500;
  padding: 8px 16px;
  border: 1px solid #2196f3;
  border-radius: 4px;
  transition: all 0.3s;
}

.checkout-link:hover {
  background: #2196f3;
  color: white;
}

.dialog-footer {
  padding: 20px;
  border-top: 1px solid #e0e0e0;
  text-align: right;
}

.cancel-button {
  padding: 10px 24px;
  background: #f5f5f5;
  border: none;
  border-radius: 4px;
  font-size: 14px;
  font-weight: 500;
  color: #666;
  cursor: pointer;
  transition: background 0.3s;
}

.cancel-button:hover {
  background: #e0e0e0;
}

.loading {
  text-align: center;
  padding: 40px;
}

.loading p {
  font-size: 16px;
  color: #666;
}

.error-message {
  text-align: center;
  padding: 20px;
}

.error-message p {
  color: #f44336;
  margin-bottom: 15px;
}

.error-message button {
  padding: 10px 24px;
  background: #2196f3;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.3s;
}

.error-message button:hover {
  background: #1976d2;
}
```

## Summary

These examples demonstrate:

1. ✅ Creating QR payment links via API
2. ✅ Generating QR codes from checkout URLs
3. ✅ Polling for payment status updates
4. ✅ Handling success and error cases
5. ✅ Providing alternative payment link
6. ✅ Clean UI/UX with loading and error states

Choose the framework that matches your project and adapt as needed!
