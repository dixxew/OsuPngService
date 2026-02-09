pipeline {
    agent any
    
    environment {
        DOCKER_IMAGE = 'osupng-service'
        COMPOSE_FILE = 'docker-compose.yml'
    }
    
    stages {
        stage('Checkout') {
            steps {
                echo 'Pulling latest code...'
                checkout scm
            }
        }
        
        stage('Build') {
            steps {
                echo 'Building Docker image...'
                sh 'docker compose build'
            }
        }
        
        stage('Deploy') {
            steps {
                echo 'Deploying service...'
                sh 'docker compose down'
                sh 'docker compose up -d'
            }
        }
        
        stage('Health Check') {
            steps {
                echo 'Waiting for service to start...'
                script {
                    sleep(time: 10, unit: 'SECONDS')
                    
                    def maxRetries = 5
                    def success = false
                    
                    for (int i = 0; i < maxRetries; i++) {
                        def response = sh(
                            script: 'docker exec osupng-service curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/osupng || echo "FAIL"',
                            returnStdout: true
                        ).trim()
                        
                        if (response == '200') {
                            echo "✅ Service is healthy (HTTP ${response})"
                            success = true
                            break
                        }
                        
                        echo "Attempt ${i+1}/${maxRetries}: HTTP ${response}, retrying..."
                        sleep(time: 5, unit: 'SECONDS')
                    }
                    
                    if (!success) {
                        error("Service health check failed after ${maxRetries} attempts")
                    }
                }
            }
        }
    }
    
    post {
        success {
            echo '✅ OsuPngService deployed successfully!'
        }
        failure {
            echo '❌ Deployment failed!'
            sh 'docker compose logs --tail=100 osupng-service'
        }
        always {
            echo 'Cleaning up unused Docker images...'
            sh 'docker image prune -f'
        }
    }
}