%% Mermaid approximation of the use-case diagram
graph LR
  subgraph Actors
    Visitor[(Visitor)]
    User[(User)]
    Admin[(Admin)]
    Manager[(Manager)]
  end

  subgraph UseCases
    ViewCatalog(("View Catalog"))
    Register(("Register"))
    Login(("Login"))
    CreateOrder(("Create Order"))
    TrackOrder(("Track Order"))
    ManageProducts(("Manage Products"))
    EditProduct(("Edit Product"))
    ManageOrders(("Manage Orders"))
  end

  Visitor --> ViewCatalog
  Visitor --> Register
  Visitor --> Login

  User --> CreateOrder
  User --> TrackOrder

  Admin --> ManageProducts
  Admin --> EditProduct

  Manager --> ManageOrders

  Register -.-> CreateOrder
  Login -.-> CreateOrder
