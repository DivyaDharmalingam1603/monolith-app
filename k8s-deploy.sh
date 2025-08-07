#!/bin/bash

# Exit immediately if a command exits with a non-zero status
set -e

# Take the image tag as a parameter (this you provide when running the script)
IMAGE_TAG=$1
if [ -z "$IMAGE_TAG" ]; then
  echo "Error: IMAGE_TAG parameter is required"
  echo "Usage: ./k8s-deploy.sh <image-tag>"
  exit 1
fi

# === CHANGE HERE ===
# Replace with your actual AWS ECR repository URI where your Docker images are stored.
# Format: <aws_account_id>.dkr.ecr.<region>.amazonaws.com/<repository_name>
REPOSITORY_URI="148336247604.dkr.ecr.us-east-1.amazonaws.com/hackathon"

# === CHANGE HERE ===
# Your AWS region (e.g., us-east-1, us-west-2)
AWS_REGION="us-east-1"

# === CHANGE HERE ===
# Amazon EKS cluster name you want to deploy to
EKS_CLUSTER_NAME="legacy-cluster"

# === CHANGE HERE ===
# Kubernetes Deployment name in your cluster
# Make sure this matches your deployment's metadata.name in Kubernetes manifests
DEPLOYMENT_NAME="my-dotnet-app"

echo "Updating kubeconfig for EKS cluster '${EKS_CLUSTER_NAME}' in region '${AWS_REGION}'..."
aws eks update-kubeconfig --region "$AWS_REGION" --name "$EKS_CLUSTER_NAME"

echo "Updating Kubernetes deployment '${DEPLOYMENT_NAME}' with new image: ${REPOSITORY_URI}:${IMAGE_TAG}..."
# kubectl sets the image for the specified deployment and container
# The container name used here is assumed to be the same as DEPLOYMENT_NAME.
# If your container name inside the deployment differs, change the second "$DEPLOYMENT_NAME" to your container name.
kubectl set image deployment/"$DEPLOYMENT_NAME" "$DEPLOYMENT_NAME"="$REPOSITORY_URI:$IMAGE_TAG"

echo "Waiting for rollout to complete..."
kubectl rollout status deployment/"$DEPLOYMENT_NAME"

echo "Deployment successful!"
