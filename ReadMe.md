# Task

This is a small exercise based around a test I once did and have subsquently modified to bear no association. I am also using it to experiment with differnt technologies or design patterns and practices.

The original problem was to design a system to model the relationship structure of a royal family. and provide the ability to determine the relationship between two individuals given two names as input.

I have chosen to design the system in as a .NET Core solution using MVC and Entity Framework as a data model. The code will be structured as a 3-tier design using dependency injection to facilitate unit testing and allow extensibility. For all purposes this design is a little contrived for this simple scenario but is emulates a possible design of a real-world system.

# Designg

## System requirements

* An operating system supported by .NET Core (e.g. Windows, Linux, etc.)
* .NET Core 2.1

## Requirements

* Model the relationships of a royal family
* Given two names as input output the relationships between two family members (e.g. Mother, Brother-in-law, Paternal uncle, etc.)
* Well modelled, readable, extensible code that follows good OOPS concepts. 
* Provide unit tests and build scripts

## Assumptions

The following assumptions were determined by analysing the dataset given, or individually assumed, in absence of any specific requirements.

* Each person has a name.
* Each person may have 2 parents.
* Each person may have a partner.
* Each person may have 0 or more children.
* Each person, or their spouse should relate directly or indirectly to King and Queen.
* Each person has a gender, either Male or Female.
* Each person may be Royal by birth.
* A relationship is between 2 people.
* Two people in a relationship are married.
* A Person's name is sufficiently unique in this example
* The model may contain relationships for people who are not royal by birth but are married to a royal person.

# Usage

1. Build the solution
2. Run the tests
3. Run the self-hosted website
4. Enjoy