# 📊 **SCHÉMA VISUEL DES DÉPENDANCES - NIESPRО ERP**

**Architecture Microservices** : .NET 8 + MySQL + Clean Architecture

---

## 🏗️ **DIAGRAMME DES DÉPENDANCES PAR NIVEAUX**

```mermaid
graph TD
    %% NIVEAU 0 - Infrastructure de base
    subgraph "NIVEAU 0 - Infrastructure Base"
        LOGS[Logs.API<br/>📋 Port 5018<br/>NiesPro_Logs + Elasticsearch<br/>✅ PRODUCTION READY]
    end

    %% NIVEAU 1 - Infrastructure Core  
    subgraph "NIVEAU 1 - Infrastructure Core"
        GATEWAY[Gateway.API<br/>🌐 Port 5010<br/>Proxy/Router<br/>✅ PRODUCTION READY]
        AUTH[Auth.API<br/>🔐 Port 5011<br/>niespro_auth<br/>✅ PRODUCTION READY]
    end

    %% NIVEAU 2 - Services Core Autonomes
    subgraph "NIVEAU 2 - Services Core"
        CATALOG[Catalog.API<br/>🛍️ Port 5013<br/>niespro_catalog<br/>✅ PRODUCTION READY]
        STOCK[Stock.API<br/>📦 Port 5015<br/>NiesPro_Stock<br/>🔄 EN MIGRATION]
        CUSTOMER[Customer.API<br/>👥 Port 5016<br/>NiesPro_Customer<br/>🚨 DUPLICATION]
    end

    %% NIVEAU 3 - Services Business
    subgraph "NIVEAU 3 - Services Business"
        RESTAURANT[Restaurant.API<br/>🍽️ Port 5017<br/>NiesPro_Restaurant<br/>🔄 EN DEV 40%]
        ORDER[Order.API<br/>📋 Port 5012<br/>NiesPro_Order + EventStore<br/>✅ PRODUCTION READY]
        PAYMENT[Payment.API<br/>💳 Port 5014<br/>NiesPro_Payment<br/>🔄 EN COURS]
    end

    %% NIVEAU 4 - Services Avancés
    subgraph "NIVEAU 4 - Services Avancés"
        NOTIFICATION[Notification.API<br/>📢 Port 5030<br/>NiesPro_Notifications<br/>❌ NON IMPLÉMENTÉ]
        REPORT[Report.API<br/>📊 Port 5031<br/>NiesPro_Reports<br/>❌ CRITIQUE BUSINESS]
        FILE[File.API<br/>📁 Port 5032<br/>NiesPro_Files + Cloud<br/>❌ NON IMPLÉMENTÉ]
        INTEGRATION[Integration.API<br/>🔌 Port 5033<br/>NiesPro_Integrations<br/>❌ EXTENSION]
        BACKUP[Backup.API<br/>💾 Port 5034<br/>NiesPro_Backup<br/>❌ COMPLIANCE]
    end

    %% Dépendances NIVEAU 0 → NIVEAU 1
    LOGS --> GATEWAY
    LOGS --> AUTH

    %% Dépendances NIVEAU 1 → NIVEAU 2
    LOGS --> CATALOG
    LOGS --> STOCK  
    LOGS --> CUSTOMER

    %% Dépendances NIVEAU 2 → NIVEAU 3
    AUTH --> RESTAURANT
    CATALOG --> RESTAURANT
    STOCK --> RESTAURANT
    LOGS --> RESTAURANT

    AUTH --> ORDER
    CATALOG --> ORDER
    STOCK --> ORDER
    CUSTOMER --> ORDER
    LOGS --> ORDER

    AUTH --> PAYMENT
    ORDER --> PAYMENT
    CUSTOMER --> PAYMENT
    LOGS --> PAYMENT

    %% Dépendances NIVEAU 3 → NIVEAU 4
    AUTH --> NOTIFICATION
    CUSTOMER --> NOTIFICATION
    ORDER --> NOTIFICATION
    STOCK --> NOTIFICATION
    LOGS --> NOTIFICATION

    AUTH --> REPORT
    ORDER --> REPORT
    STOCK --> REPORT
    CUSTOMER --> REPORT
    PAYMENT --> REPORT
    LOGS --> REPORT

    AUTH --> FILE
    LOGS --> FILE

    AUTH --> INTEGRATION
    LOGS --> INTEGRATION
    CATALOG --> INTEGRATION
    STOCK --> INTEGRATION
    ORDER --> INTEGRATION
    CUSTOMER --> INTEGRATION

    LOGS --> BACKUP

    %% Styles par statut
    classDef productionReady fill:#4CAF50,stroke:#2E7D32,stroke-width:3px,color:#fff
    classDef enCours fill:#FF9800,stroke:#F57C00,stroke-width:2px,color:#fff  
    classDef nonImplemente fill:#F44336,stroke:#C62828,stroke-width:2px,color:#fff
    classDef duplication fill:#9C27B0,stroke:#6A1B9A,stroke-width:2px,color:#fff

    class LOGS,GATEWAY,AUTH,CATALOG,ORDER productionReady
    class STOCK,RESTAURANT,PAYMENT enCours
    class NOTIFICATION,REPORT,FILE,INTEGRATION,BACKUP nonImplemente
    class CUSTOMER duplication
```

---

## 📋 **LÉGENDE DES STATUTS**

| Couleur | Status | Description |
|---------|--------|-------------|
| 🟢 **Vert** | **PRODUCTION READY** | Service opérationnel avec tests complets |
| 🟠 **Orange** | **EN COURS** | Service en développement ou migration |
| 🔴 **Rouge** | **NON IMPLÉMENTÉ** | Service planifié mais pas encore développé |
| 🟣 **Violet** | **DUPLICATION** | Problème architectural à résoudre |

---

## 🔗 **MATRICE DES DÉPENDANCES DÉTAILLÉE**

### **📊 Table des Relations**

| Service Source | Dépendances Directes | Type de Relation | Criticité |
|---------------|---------------------|------------------|-----------|
| **Logs.API** | AUCUNE | Infrastructure | 🟢 FONDATION |
| **Gateway.API** | Logs.API | Proxy | 🟢 CRITIQUE |
| **Auth.API** | Logs.API | Audit | 🟢 CRITIQUE |
| **Catalog.API** | Logs.API | Audit | 🟢 AUTONOME |
| **Stock.API** | Logs.API | Audit | 🟠 AUTONOME |
| **Customer.API** | Logs.API | Audit | 🟣 DUPLICATION |
| **Restaurant.API** | Auth + Catalog + Stock + Logs | Business | 🟠 COMPLEXE |
| **Order.API** | Auth + Catalog + Stock + Customer + Logs | Business | 🟢 COMPLEXE |
| **Payment.API** | Auth + Order + Customer + Logs | Business | 🟠 COMPLEXE |
| **Notification.API** | Auth + Customer + Order + Stock + Logs | Support | 🔴 MULTI-DEPS |
| **Report.API** | Auth + Order + Stock + Customer + Payment + Logs | Analytics | 🔴 MULTI-DEPS |
| **File.API** | Auth + Logs | Utility | 🔴 SIMPLE |
| **Integration.API** | Tous services métier + Logs | Extension | 🔴 COMPLEXE |
| **Backup.API** | Tous services + Logs | Infrastructure | 🔴 GLOBAL |

---

## 🎯 **ORDRE D'IMPLÉMENTATION OPTIMAL**

### **🚀 SÉQUENCE RECOMMANDÉE**

#### **Phase Actuelle - Consolidation** ✅
```
1. Logs.API        ✅ TERMINÉ
2. Gateway.API     ✅ TERMINÉ  
3. Auth.API        ✅ TERMINÉ
4. Catalog.API     ✅ TERMINÉ
5. Order.API       ✅ TERMINÉ
```

#### **Phase Urgente - Services Core** 🔥
```
6. Customer.API    🚨 RÉSOUDRE DUPLICATION
7. Stock.API       🔄 FINALISER MIGRATION  
8. Payment.API     🔄 CORRIGER WARNINGS
```

#### **Phase Business - Services Métier** 📈
```
9. Restaurant.API   🔄 COMPLÉTER DÉVELOPPEMENT
10. Report.API      ❌ CRITIQUE BUSINESS
11. Notification.API ❌ EXPÉRIENCE UTILISATEUR
```

#### **Phase Extension - Services Avancés** 🚀
```
12. File.API        ❌ GESTION DOCUMENTS
13. Integration.API ❌ APIS EXTERNES  
14. Backup.API      ❌ COMPLIANCE
```

---

## ⚡ **POINTS DE BLOCAGE IDENTIFIÉS**

### **🚨 BLOQUANTS CRITIQUES**
1. **Customer Duplication** : Empêche finalisation Order.API et Payment.API
2. **Stock.API Migration** : Requis par Restaurant.API et Order.API  
3. **Payment.API Warnings** : 20 warnings de compilation à corriger

### **📋 DÉPENDANCES CRITIQUES**
- **Order.API** dépend de 5 services (plus complexe)
- **Payment.API** dépend de Order.API (cascade)
- **Report.API** dépend de tous les services métier

### **🎯 CHEMIN CRITIQUE**
```
Customer.API (résolution duplication) 
    ↓
Order.API (finalisation complète)
    ↓  
Payment.API (correction warnings)
    ↓
Restaurant.API (développement métier)
    ↓
Report.API (analytics business)
```

---

## 📊 **IMPACT ANALYSE**

### **🎯 Services à Impact Élevé**
- **Logs.API** : Impact sur 100% des services (fondation)
- **Auth.API** : Impact sur tous services business (sécurité)
- **Order.API** : Impact sur Payment et Report (workflow central)

### **🔗 Services Fortement Couplés**  
- **Order ↔ Payment** : Workflow transactionnel
- **Restaurant → Catalog + Stock** : Gestion ingrédients
- **Report → Tous** : Agrégation de données

### **⚡ Services Autonomes**
- **Catalog.API** : Peut fonctionner indépendamment
- **File.API** : Service utility standalone
- **Backup.API** : Infrastructure indépendante

---

**🏗️ Cette architecture respecte les principes microservices avec une séparation claire des responsabilités et une évolutivité maximale.**