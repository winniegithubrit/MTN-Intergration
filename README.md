# MTN-Intergration
intergrating MTN momo using c#
# Airtel-MTN-CBS
# CONFIGUTING THE DATABASE (MYSQL DATABASE )

# INSTALL MYSQL :
sudo apt-get install mysql-server

# Secure MySQL Installation:
sudo mysql_secure_installation

<!-- # Start MySQL
sudo service mysql start

# Verify Installation: 
mysql -u root -p -->

# Log in to MySQL 
sudo mysql -u root

# Set the root password and configure MySQL to use password authentication for the root user:
ALTER USER 'root'@'localhost' IDENTIFIED WITH mysql_native_password BY 'winnieko';

# Flush the privileges to ensure that the changes take effect:
FLUSH PRIVILEGES;

# Exit MySQL:
EXIT;

# log in to MySQL using the new root password:
mysql -u root -p

# Check Users and Authentication Methods:
SELECT user, host, plugin FROM mysql.user WHERE user = 'root';
# Create a Database 
CREATE DATABASE Banked;

# Create a New User FOR THE DATABASE:
CREATE USER 'Ayitso'@'localhost' IDENTIFIED BY '!Jomo@17##';
GRANT ALL PRIVILEGES ON BankingSystem.* TO 'Ayitso'@'localhost';
FLUSH PRIVILEGES;



# RELATIONSHIPS
# User Model:

One user can have multiple transactions, defined by the ICollection<Transaction> Transactions property.
ONE TO MANY RELATIONSHIP
# Transaction Model:

Each transaction belongs to one user, defined by the User User property. (ONE TO ONE )
Each transaction has one status, defined by the TransactionStatus Status property.(ONE TO ONE)
Each transaction can have multiple additional infos, defined by the ICollection<AdditionalInfo> AdditionalInfos property.(ONE TO MANY RELATIONSHIP)
# TransactionStatus Model:

Each status can be associated with multiple transactions, defined by the ICollection<Transaction> Transactions property.(ONE TO MANY)
# AdditionalInfo Model:

Each additional info is related to one transaction, defined by the Transaction Transaction property.(ONE TO ONE)

# RUNNING MIGRATIONS 
# Install Entity Framework Core Tools:
dotnet tool install --global dotnet-ef
# Verify Installation:
dotnet ef --version


Ensure Application Builds Successfully:
dotnet run

# Create Migrations:
dotnet ef migrations add InitialCreate

# Apply Migrations to Database:
dotnet ef database update
# oy have to identify the primary key attribute using
[key]
 # Install dotnet-ef as a Local Tool(this is done before the migrations commands incase of any errors)
dotnet new tool-manifest 
dotnet tool install dotnet-ef --local
dotnet tool run dotnet-ef --version
dotnet ef --version
# after this run the migrations command
# seeding
adding data to the database that we will use to test
# Add a New Migration: Create a new migration to incorporate the seeding changes.
dotnet ef migrations add SeedData
# Apply the Migration: Apply the new migration to update the database with the seeded data.
dotnet ef database update
# Run Your Application:
dotnet run
# AIRTEL AND MTN INTERGRATION:
# authentication(obtaining the secret key and client id)
testing the code using postman
# export PATH="$PATH:/home/nathalie/.dotnet/tools"


