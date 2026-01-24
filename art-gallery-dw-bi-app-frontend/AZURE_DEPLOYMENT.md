# 🎨 Azure Static Web Apps Deployment Guide

This guide will help you deploy the Art Gallery Management System to Azure Static Web Apps (Free Tier).

## 📋 Prerequisites

1. **Azure Account**: [Create a free Azure account](https://azure.microsoft.com/free/)
2. **GitHub Account**: Your code should be in a GitHub repository
3. **Azure CLI** (optional): For command-line deployment

## 🆓 Free Tier Limits

Azure Static Web Apps Free Tier includes:
- ✅ **100 GB bandwidth** per month
- ✅ **0.5 GB storage** per app
- ✅ **Free SSL certificates**
- ✅ **Custom domains**
- ✅ **GitHub/Azure DevOps integration**
- ✅ **Unlimited environments** (staging, production)

## 🚀 Deployment Steps

### Method 1: Azure Portal (Recommended for Beginners)

#### Step 1: Push to GitHub
```bash
# Initialize git repository (if not already done)
git init

# Add all files
git add .

# Commit
git commit -m "Initial commit - Art Gallery App"

# Add remote repository (replace with your GitHub repo URL)
git remote add origin https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git

# Push to GitHub
git push -u origin main
```

#### Step 2: Create Static Web App in Azure Portal

1. Go to [Azure Portal](https://portal.azure.com)
2. Click **"Create a resource"**
3. Search for **"Static Web App"**
4. Click **"Create"**
5. Fill in the details:
   - **Subscription**: Select your subscription
   - **Resource Group**: Create new or use existing
   - **Name**: `art-gallery-app` (must be unique)
   - **Plan type**: **Free**
   - **Region**: Choose closest to your users
   - **Deployment source**: **GitHub**
   
6. Click **"Sign in with GitHub"** and authorize Azure

7. Select your repository:
   - **Organization**: Your GitHub username
   - **Repository**: Your repo name
   - **Branch**: `main` or `master`

8. **Build Details**:
   - **Build Presets**: Select **"Vue.js"**
   - **App location**: `/` (root)
   - **Api location**: Leave empty
   - **Output location**: `dist`

9. Click **"Review + Create"** then **"Create"**

#### Step 3: Wait for Deployment

- Azure will automatically:
  - Create a GitHub Actions workflow in your repo
  - Build and deploy your app
  - Provide you with a URL like: `https://wonderful-cliff-123abc.azurestaticapps.net`

- Check the **GitHub Actions** tab in your repository to monitor progress

#### Step 4: Configure Environment Variables (if needed)

1. In Azure Portal, go to your Static Web App resource
2. Click **"Configuration"** in the left menu
3. Under **"Application settings"**, add:
   - Name: `VITE_API_BASE_URL`
   - Value: Your backend API URL
4. Click **"Save"**

---

### Method 2: Azure CLI

```bash
# Install Azure CLI (if not installed)
# Windows: Download from https://aka.ms/installazurecliwindows

# Login to Azure
az login

# Install Static Web Apps extension
az extension add --name staticwebapp

# Create resource group
az group create --name art-gallery-rg --location eastus

# Create Static Web App (replace values)
az staticwebapp create \
  --name art-gallery-app \
  --resource-group art-gallery-rg \
  --source https://github.com/YOUR_USERNAME/YOUR_REPO \
  --location eastus \
  --branch main \
  --app-location "/" \
  --output-location "dist" \
  --login-with-github
```

---

## 🔧 Important Configuration Files

The following files have been created for you:

### 1. `staticwebapp.config.json`
- Configures SPA routing (all routes redirect to index.html)
- Sets security headers
- Handles 404 errors

### 2. `.github/workflows/azure-static-web-apps.yml`
- Automated CI/CD pipeline
- Builds and deploys on every push to main/master
- Creates preview deployments for pull requests

### 3. `.env.example`
- Template for environment variables
- Copy to `.env` for local development

---

## 🌐 Custom Domain (Optional)

To add a custom domain:

1. In Azure Portal, go to your Static Web App
2. Click **"Custom domains"**
3. Click **"Add"**
4. Enter your domain name
5. Follow DNS configuration instructions
6. Azure automatically provisions SSL certificate

---

## 🔄 Update Deployment

Simply push to your GitHub repository:

```bash
git add .
git commit -m "Your changes"
git push
```

GitHub Actions will automatically rebuild and deploy.

---

## 🧪 Local Testing Before Deployment

```bash
# Install dependencies
npm install

# Run development server
npm run dev

# Build for production
npm run build

# Preview production build locally
npm run preview
```

---

## ⚠️ Important Notes

### Backend API
Your app currently uses `json-server` for local development. For production:

1. **Option 1**: Deploy a real backend API (Azure App Service, Azure Functions)
2. **Option 2**: Use Azure Functions with Static Web Apps
3. **Option 3**: Use a third-party backend service

Update the `VITE_API_BASE_URL` environment variable accordingly.

### CORS Configuration
If your backend API is on a different domain, ensure CORS is configured:

```javascript
// Example for Express.js backend
const cors = require('cors');
app.use(cors({
  origin: 'https://your-app.azurestaticapps.net'
}));
```

---

## 📊 Monitoring

View your app analytics in Azure Portal:
- **Overview**: Deployment history, URL
- **Functions**: API endpoints (if using Azure Functions)
- **Configuration**: Environment variables
- **Custom domains**: Domain management
- **Environments**: Preview deployments

---

## 💰 Cost Monitoring

Free tier limits:
- Monitor bandwidth usage in Azure Portal
- Upgrade to Standard tier ($9/month) if you exceed limits

---

## 🐛 Troubleshooting

### Build fails on Azure
- Check GitHub Actions logs
- Verify `package.json` scripts
- Ensure Node.js version compatibility

### Routing issues (404 errors)
- Verify `staticwebapp.config.json` is present
- Check `navigationFallback` configuration

### Environment variables not working
- In Azure Portal, ensure they're prefixed with `VITE_`
- Rebuild after adding variables

---

## 📚 Additional Resources

- [Azure Static Web Apps Documentation](https://docs.microsoft.com/azure/static-web-apps/)
- [Pricing Details](https://azure.microsoft.com/pricing/details/app-service/static/)
- [GitHub Actions Workflow](https://docs.github.com/actions)

---

## 🎉 Your App URL

After deployment, your app will be available at:
```
https://<your-app-name>.azurestaticapps.net
```

You can find the exact URL in:
- Azure Portal → Your Static Web App → Overview
- GitHub Actions → Latest workflow run → Deploy step
