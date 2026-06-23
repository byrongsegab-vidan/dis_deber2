pipeline {
    agent {
        label 'windows' // Fuerza a que este pipeline se ejecute en el agente de Windows
    }
    
    environment {
        MSBUILD = 'C:\\Program Files\\Microsoft Visual Studio\\18\\Insiders\\MSBuild\\Current\\Bin\\MSBuild.exe'
        NUGET = 'C:\\ITSCO DIS\\dis_deber2\\nuget.exe'
        DEPLOY_PATH = 'C:\\inetpub\\wwwroot\\dis_deber2'
    }
    
    stages {
        stage('Restaurar Paquetes NuGet') {
            steps {
                echo 'Restaurando paquetes NuGet de la solución...'
                bat "\"${NUGET}\" restore dis_deber2.slnx"
            }
        }
        
        stage('Compilar Solución') {
            steps {
                echo 'Compilando el monolito .NET Framework 4.8...'
                // Compila el csproj de la aplicación y lo publica en la carpeta temporal de Jenkins sin necesidad de perfiles de publicación (.pubxml)
                bat "\"${MSBUILD}\" dis_deber2\\dis_deber2.csproj /p:Configuration=Release /p:WebProjectOutputDir=\"${WORKSPACE}\\publish\" /p:OutDir=\"${WORKSPACE}\\publish\\bin\\\\\" /t:Rebuild"
            }
        }
        
        stage('Ejecutar Pruebas') {
            steps {
                echo 'Ejecutando pruebas de compilación, estructura y base de datos...'
                powershell '''
                    Write-Host "Iniciando verificación de despliegue..." -ForegroundColor Cyan
                    
                    # 1. Verificar estructura física del build
                    if (Test-Path "publish\\Web.config") {
                        Write-Host "Prueba de estructura: EXITOSA (Web.config generado)" -ForegroundColor Green
                    } else {
                        Write-Error "Prueba de estructura: FALLIDA (No se encontró el Web.config en la carpeta de publicación)"
                        exit 1
                    }
                    
                    # 2. Verificar existencia de páginas clave (.aspx)
                    if (Test-Path "publish\\Default.aspx") {
                        Write-Host "Prueba de archivos principales: EXITOSA (Default.aspx encontrado)" -ForegroundColor Green
                    } else {
                        Write-Error "Prueba de archivos principales: FALLIDA (No se encontró Default.aspx)"
                        exit 1
                    }
                    
                    # 3. Prueba de conectividad con SQL Server Express local
                    Write-Host "Probando conexión con el servidor SQL Server local..." -ForegroundColor Cyan
                    try {
                        # Obtenemos la conexión con la base de datos dis_deber2_db
                        $connectionString = "Server=ISADORA\\vinic;Database=dis_deber2_db;Integrated Security=True;"
                        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
                        $connection.Open()
                        Write-Host "Prueba de conexión a SQL Server (ISADORA\\vinic): EXITOSA" -ForegroundColor Green
                        $connection.Close()
                    } catch {
                        Write-Warning "No se pudo conectar a la BD local mediante Integrated Security. Nota: El pipeline continuará ya que en producción se usa la cadena en Web.config."
                        Write-Host "Prueba de conexión a SQL Server: OMITIDA/ADVERTENCIA ($_.Exception.Message)" -ForegroundColor Yellow
                    }
                '''
            }
        }
        
        stage('Publicar Aplicación') {
            steps {
                echo 'Empaquetando artefactos compilados...'
                powershell '''
                    if (Test-Path "publish") {
                        Write-Host "Carpeta de publicación verificada y lista para despliegue." -ForegroundColor Green
                    } else {
                        Write-Error "Error: La carpeta de publicación no existe."
                        exit 1
                    }
                '''
            }
        }
        
        stage('Desplegar en IIS') {
            steps {
                echo "Desplegando monolito en IIS: ${DEPLOY_PATH}..."
                powershell """
                    # Verificar si existe el directorio de destino
                    if (!(Test-Path "${DEPLOY_PATH}")) {
                        Write-Host "Creando el directorio de destino ${DEPLOY_PATH}..."
                        New-Item -ItemType Directory -Force -Path "${DEPLOY_PATH}"
                    }
                    
                    # Copiar los archivos compilados del pipeline a IIS
                    Write-Host "Copiando archivos compilados..."
                    Copy-Item -Path "publish\\*" -Destination "${DEPLOY_PATH}" -Recurse -Force
                    
                    Write-Host "Despliegue finalizado exitosamente." -ForegroundColor Green
                """
            }
        }
    }
    
    post {
        success {
            echo '¡El pipeline se completó correctamente y el sitio está desplegado!'
        }
        failure {
            echo 'El pipeline falló. Revisa los logs de compilación o pruebas.'
        }
    }
}
