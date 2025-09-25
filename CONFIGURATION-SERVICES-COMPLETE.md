# 🔧 CONFIGURATION COMPLÈTE DES SERVICES - NIESPRO

*Analyse exhaustive des fichiers de configuration - 25 Septembre 2025*

---

## 📊 **TABLEAU COMPLET DES CONFIGURATIONS**

| Service | HTTP Port | HTTPS Port | Base de Données | launchSettings.json | appsettings.json | Incohérences Détectées |
|---------|-----------|------------|-----------------|---------------------|------------------|------------------------|
| **Auth** | 5001 | **5011** | niespro_auth | ✅ 5001/7001 | ✅ 5001/5011 | 🚨 **HTTPS: 7001 ≠ 5011** |
| **Catalog** | 5003 | **5013** | niespro_catalog | ✅ 5003/7003 | ✅ 5003/5013 | 🚨 **HTTPS: 7003 ≠ 5013** |
| **Order** | 5002 | **5012** | NiesPro_Order + EventStore | ✅ 5002/7002 | ✅ 5002/5012 | 🚨 **HTTPS: 7002 ≠ 5012** |
| **Payment** | 5004 | **5014** | NiesPro_Payment | ✅ 5004/7146 | ✅ 5004/5014 | 🚨 **HTTPS: 7146 ≠ 5014** |
| **Stock** | 5005 | **5006** | NiesPro_Stock | ✅ 5005/7005 | ✅ 5005/5006 | 🚨 **HTTPS: 7005 ≠ 5006** |
| **Restaurant** | **7001** | **7011** | NiesPro_Restaurant | ❌ Manquant | ✅ 7001/7011 | 🚨 **HTTP: 7001 conflit avec Auth HTTPS** |
| **Customer.API** | **8001** | **8011** | NiesPro_Customer | ❌ Manquant | ✅ 8001/8011 | ⚠️ Plage ports différente |
| **CustomerService** | ❌ Non configuré | ❌ Non configuré | NiesPro_Customer | ❌ Manquant | ❌ Pas de Kestrel | 🚨 **Service fantôme** |
| **Gateway** | **5000** | **5010** | Aucune (proxy) | ❌ Manquant | ✅ 5000/5010 | ✅ Cohérent |

---

## 🚨 **INCOHÉRENCES CRITIQUES DÉTECTÉES**

### **1. CONTRADICTION MAJEURE : launchSettings.json vs appsettings.json**

Chaque service a **DEUX configurations différentes** pour les ports HTTPS :

#### **Auth Service :**
- **launchSettings.json :** 5001 (HTTP) / **7001** (HTTPS)
- **appsettings.json :** 5001 (HTTP) / **5011** (HTTPS)
- **Problème :** 7001 ≠ 5011

#### **Catalog Service :**
- **launchSettings.json :** 5003 (HTTP) / **7003** (HTTPS)  
- **appsettings.json :** 5003 (HTTP) / **5013** (HTTPS)
- **Problème :** 7003 ≠ 5013

#### **Order Service :**
- **launchSettings.json :** 5002 (HTTP) / **7002** (HTTPS)
- **appsettings.json :** 5002 (HTTP) / **5012** (HTTPS)  
- **Problème :** 7002 ≠ 5012

#### **Payment Service :**
- **launchSettings.json :** 5004 (HTTP) / **7146** (HTTPS) 
- **appsettings.json :** 5004 (HTTP) / **5014** (HTTPS)
- **Problème :** 7146 ≠ 5014 (double incohérence)

#### **Stock Service :**
- **launchSettings.json :** 5005 (HTTP) / **7005** (HTTPS)
- **appsettings.json :** 5005 (HTTP) / **5006** (HTTPS)
- **Problème :** 7005 ≠ 5006

### **2. CONFLIT DE PORTS CRITIQUE**
- **Restaurant Service HTTP (7001)** = **Auth Service HTTPS (7001)**
- **Impact :** Impossible de démarrer les deux services simultanément

### **3. SERVICES SANS CONFIGURATION**
- **CustomerService** : Aucun port configuré (service fantôme)
- **Restaurant** : Pas de launchSettings.json
- **Customer.API** : Pas de launchSettings.json  
- **Gateway** : Pas de launchSettings.json

---

## 🔍 **ANALYSE PAR SERVICE**

### **✅ Auth Service (Port 5001)**
```json
// appsettings.json
"Kestrel": {
  "Endpoints": {
    "Http": { "Url": "http://localhost:5001" },
    "Https": { "Url": "https://localhost:5011" }
  }
}

// launchSettings.json  
"https": {
  "applicationUrl": "https://localhost:7001;http://localhost:5001"
}
```
**Configuration :** JWT complet, Device Keys, Base niespro_auth ✅  
**Problème :** HTTPS 7001 vs 5011 🚨

### **✅ Catalog Service (Port 5003)**
```json
// appsettings.json
"Kestrel": {
  "Endpoints": {
    "Http": { "Url": "http://localhost:5003" },
    "Https": { "Url": "https://localhost:5013" }
  }
}

// launchSettings.json
"https": {
  "applicationUrl": "https://localhost:7003;http://localhost:5003"  
}
```
**Configuration :** Base niespro_catalog ✅  
**Problème :** HTTPS 7003 vs 5013 🚨

### **✅ Order Service (Port 5002)**
```json
// appsettings.json
"Kestrel": {
  "Endpoints": {
    "Http": { "Url": "http://localhost:5002" },
    "Https": { "Url": "https://localhost:5012" }
  }
}
```
**Configuration :** Event Sourcing + CQRS, Base NiesPro_Order + EventStore ✅  
**Problème :** HTTPS 7002 vs 5012 🚨

### **⚠️ Payment Service (Port 5004)**
```json
// appsettings.json
"Kestrel": {
  "Endpoints": {
    "Http": { "Url": "http://localhost:5004" },
    "Https": { "Url": "https://localhost:5014" }
  }
}
```
**Configuration :** PCI DSS complet, Stripe/PayPal/Crypto ready ✅  
**Problème :** Triple incohérence (7146 vs 5014 vs logique 7004) 🚨

### **⚠️ Stock Service (Port 5005)**
```json
// appsettings.json  
"Kestrel": {
  "Endpoints": {
    "Http": { "Url": "http://localhost:5005" },
    "Https": { "Url": "https://localhost:5006" }
  }
}
```
**Configuration :** Base NiesPro_Stock ✅  
**Problème :** HTTPS 7005 vs 5006 🚨

### **🚨 Restaurant Service (Port 7001)**
```json
// appsettings.json SEULEMENT
"Kestrel": {
  "Endpoints": {
    "Http": { "Url": "http://localhost:7001" },    // CONFLIT avec Auth HTTPS !
    "Https": { "Url": "https://localhost:7011" }
  }
}
```
**Configuration :** JWT + CORS + Rate Limiting ✅  
**Problème Critique :** Port 7001 = Auth Service HTTPS 🚨

### **🚨 Customer.API (Port 8001)**
```json
// appsettings.json SEULEMENT
"Kestrel": {
  "Endpoints": {
    "Http": { "Url": "http://localhost:8001" },
    "Https": { "Url": "https://localhost:8011" }
  }
}
```
**Configuration :** Base NiesPro_Customer ✅  
**Problème :** Plage ports différente + duplication service 🚨

### **🚨 CustomerService (Aucun port)**
```json
// Aucune configuration Kestrel dans appsettings.json
// Aucun launchSettings.json
```
**Configuration :** Microservices dependencies, JWT, Cache Redis ✅  
**Problème :** Service fantôme sans ports 🚨

### **✅ Gateway (Port 5000)**
```json
// appsettings.json SEULEMENT
"Kestrel": {
  "Endpoints": {
    "Http": { "Url": "http://0.0.0.0:5000" },
    "Https": { "Url": "https://0.0.0.0:5010" }
  }
}
```
**Configuration :** Routing tous services, Health checks ✅  
**Problème :** Utilise ports des appsettings.json (incohérents) 🚨

---

## 🎯 **STRATÉGIE DE CORRECTION**

### **Quelle configuration prioriser ?**

**RECOMMANDATION :** Utiliser **appsettings.json** comme source de vérité

**Justification :**
1. **Runtime :** C'est appsettings.json qui est lu en production
2. **Kestrel :** Configuration explicite des endpoints
3. **Gateway :** Déjà configuré selon appsettings.json
4. **Cohérence :** Pattern 50XX/50XX+10 plus logique

### **Plan de Normalisation :**

#### **Schéma Final Recommandé :**
```
Gateway:     5000 (HTTP) / 5010 (HTTPS)
Auth:        5001 (HTTP) / 5011 (HTTPS) ✅
Order:       5002 (HTTP) / 5012 (HTTPS) ✅  
Catalog:     5003 (HTTP) / 5013 (HTTPS) ✅
Payment:     5004 (HTTP) / 5014 (HTTPS) ✅
Stock:       5005 (HTTP) / 5015 (HTTPS) 🔧 (corriger 5006→5015)
Customer:    5006 (HTTP) / 5016 (HTTPS) 🔧 (après consolidation)
Restaurant:  5007 (HTTP) / 5017 (HTTPS) 🔧 (corriger 7001→5007)
```

---

## 🔧 **ACTIONS IMMÉDIATES REQUISES**

### **Phase 1 - Corrections Critiques (1 jour)**

1. **Corriger Stock Service :**
   ```json
   "Https": { "Url": "https://localhost:5015" }  // Au lieu de 5006
   ```

2. **Corriger Restaurant Service :**
   ```json
   "Http": { "Url": "http://localhost:5007" },   // Au lieu de 7001
   "Https": { "Url": "https://localhost:5017" }  // Au lieu de 7011  
   ```

3. **Aligner launchSettings.json sur appsettings.json :**
   - Auth: 7001 → 5011
   - Catalog: 7003 → 5013  
   - Order: 7002 → 5012
   - Payment: 7146 → 5014
   - Stock: 7005 → 5015

4. **Créer launchSettings.json manquants :**
   - Restaurant, Customer.API, Gateway

### **Phase 2 - Consolidation Customer (2 jours)**

1. **Analyser duplication Customer.API vs CustomerService**
2. **Choisir Customer.API (ports 5006/5016)**  
3. **Migrer fonctionnalités CustomerService**
4. **Supprimer CustomerService redondant**

### **Phase 3 - Validation (1 jour)**

1. **Tester démarrage simultané tous services**
2. **Valider Gateway routing**  
3. **Documenter configuration finale**

---

## ❓ **QUESTION CRITIQUE**

**Quelle configuration voulez-vous prioriser :**

1. **Option A :** Garder launchSettings.json (pattern 50XX/70XX)
2. **Option B :** Garder appsettings.json (pattern 50XX/50XX+10) ← **RECOMMANDÉ**

**Voulez-vous que je procède à la normalisation selon l'Option B ?**