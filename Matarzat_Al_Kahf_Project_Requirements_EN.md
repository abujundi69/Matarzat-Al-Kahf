# Matarzat Al Kahf Project Requirements Document

## 1. Project Definition

- **Project Name:** Matarzat Al Kahf
- **System Type:** Business-to-Consumer (B2C) e-commerce store
- **Business Activity:** Selling Palestinian embroidered products and related items, such as:
  - Embroidered dresses and clothing.
  - Bags.
  - Accessories.
  - Beauty and cosmetic products.
  - Gifts and heritage products.
  - Any other products the store may decide to add in the future.

## 2. Project Objective

The Matarzat Al Kahf project aims to create an e-commerce store that displays and sells products directly to customers, with a focus on:

- Reviving Palestinian heritage and embroidery.
- Making products easier for customers to access.
- Presenting products professionally and in a mobile-friendly manner.
- Managing products, orders, and customers through an administration dashboard.
- Providing a smooth shopping experience in both Arabic and English.

---

## 3. Priorities Before Development Begins

Before starting system development, the following items must be prepared.

### 3.1 Product Preparation

The products to be offered in the first phase must be identified, and the following information should be prepared for each product:

- Product name.
- Product description.
- Product story or concept.
- Category.
- Price.
- Images.
- Available sizes.
- Available colors.
- Weight or capacity, when applicable.
- Stock quantity.
- Product status: available or unavailable.
- Whether the product is featured or included in a promotion.

### 3.2 Store Identity

The following must be finalized:

- Final store name.
- Store logo.
- Primary design colors.
- Fonts to be used.
- Product presentation style.
- Main banner images.

The Turathna website may be used as a general reference or source of design inspiration, without copying its design in full.

### 3.3 System Management Team

The people who will manage the store must be identified, along with each person’s role, such as:

- Main administrator.
- Data entry user.
- Order follow-up officer.
- Inventory officer.
- Content manager.

---

## 4. Project Implementation Phases

### 4.1 Phase One: Core Version

The first phase includes the essential functions required to operate the store:

- Display products and categories.
- Product details page.
- Shopping cart.
- Order creation.
- Payment.
- Order management.
- Product and inventory management.
- Administration dashboard.
- Arabic and English support.
- Mobile-responsive design.
- Administrator and data-entry user accounts.
- Store information pages.
- Basic notifications.
- Basic reports.

### 4.2 Phase Two: Additional Features

Some requirements may be postponed to a later phase, such as:

- Expanding the customer account system.
- Adding a wishlist.
- Saving multiple customer addresses.
- Tracking orders from the customer account.
- Product ratings and reviews.
- Advanced notifications.
- Multiple currencies.
- Advanced reports and analytics.
- Automatically synchronizing inventory with external files or systems.

---

## 5. System Design

### 5.1 Device Compatibility

The system must be highly responsive and work properly on:

- Mobile phones, which are the highest priority.
- Tablets.
- Desktop and laptop computers.
- Screens of different sizes.

The store must follow a **mobile-first** design approach, meaning the mobile experience should be designed first, followed by larger screens.

### 5.2 Languages and Layout Direction

The system supports:

- Arabic with right-to-left (**RTL**) layout.
- English with left-to-right (**LTR**) layout.

All core text must be translatable, including:

- Category names.
- Product names.
- Product descriptions.
- Page content.
- System messages.
- Order statuses.
- Banners and promotions.

---

## 6. Storefront Pages

### 6.1 Home Page

The home page should include:

- Main banner.
- Advertising banners.
- Main categories.
- Featured products.
- New products.
- Promotions and discounts.
- A brief introduction to the store.
- Social media links.
- Contact methods.
- Website footer.

The administrator must be able to control the order and visibility of selected sections from the administration dashboard.

### 6.2 Categories and Product Listings Page

Products should be displayed and filtered by:

- Main category.
- Subcategory, when needed.
- Product type.
- Price.
- Size.
- Color.
- Availability status.

Example:

`Clothing → Dresses → Hand-Embroidered Dresses`

The administrator must be able to add or edit categories and subcategories without requiring code changes.

### 6.3 Product Page

The product page should include:

- Product name.
- Product images.
- Price.
- Description.
- Product story, if available.
- Sizes.
- Colors.
- Weight or capacity.
- Availability status.
- Available quantity.
- Desired quantity selector.
- Add-to-cart button.
- Similar or recommended products.
- Delivery and payment information.

### 6.4 Shopping Cart

The shopping cart should allow customers to:

- View added products.
- Change quantities.
- Remove a product.
- View each product’s price.
- View the final total.
- Apply a discount coupon.
- Proceed to checkout.

### 6.5 Checkout and Payment Page

The checkout page must include:

- Customer information.
- Delivery address.
- Product details.
- Final amount.
- Payment method selection.
- Delivery notes field.
- Order confirmation.

### 6.6 Customer Account

The customer account may include:

- Personal information.
- Phone number.
- Addresses.
- Previous orders.
- Details of each order.
- Order status.
- Password change.
- Logout.

A simplified customer account can be implemented in the first phase and expanded later.

### 6.7 Informational Pages

The following pages must be provided:

- About Us.
- Project Story.
- The project’s goal of reviving Palestinian embroidery.
- Delivery Methods.
- Payment Methods.
- Privacy Policy.
- Exchange and Return Policy.
- Cookie Policy.
- Terms and Conditions.
- Contact Us.

The content of these pages must be editable through the administration dashboard.

---

## 7. Customer Registration and Login

### 7.1 Account Creation

Accounts are created using a phone number.

When creating an account for the first time:

1. The customer enters their phone number.
2. The system sends a one-time password (**OTP**).
3. The customer enters the OTP.
4. The phone number is verified.
5. The customer completes the account information.
6. The account is created.

### 7.2 Login

After the account has been created, the customer logs in using:

- Phone number.
- Password.

### 7.3 Forgot Password

When the customer forgets their password:

1. The customer enters their phone number.
2. The system sends an OTP.
3. The OTP is verified.
4. The customer sets a new password.

### 7.4 Guest Checkout

Before development begins, a decision must be made regarding whether customers may purchase without creating an account.

For the first phase, one of the following options is recommended:

- Purchase after logging in.
- Purchase as a guest after verifying the phone number using an OTP.

The second option may make the purchasing process faster and easier for customers.

---

## 8. Required Customer Information During Checkout

The following information is mandatory when completing an order:

- Full name.
- Mobile number.
- City or region.
- Full address.
- Delivery notes, if any.

The tax number requirement must be defined as one of the following:

- Mandatory for all customers.
- Optional.
- Required only for companies or customers requesting a tax invoice.

It is not recommended to make the tax number mandatory for all individual customers without a clear need.

---

## 9. Payment Methods

The system supports the following payment methods:

- Electronic card payments.
- Bank transfer.
- Cash on delivery.
- Electronic wallets.

The administrator must be able to:

- Enable or disable any payment method.
- Add bank transfer instructions.
- Add electronic wallet details.
- Define the cities where cash on delivery is available.
- View the selected payment method in the order details.

---

## 10. Pricing, Delivery, and Currencies

### 10.1 Delivery

According to the current requirements, the displayed product price will include the delivery cost. Therefore:

- No separate delivery fee will be added to the order.
- Customers will be informed that the price includes delivery.
- It must be determined whether this policy applies to all cities and countries.
- A process must be defined for regions with higher delivery costs.

### 10.2 Currencies

The currencies supported by the system must be defined.

A single primary currency, such as the Israeli new shekel, may be used during the first phase, with additional currencies added later.

When supporting more than one currency, the following must be defined:

- Base currency.
- Exchange-rate calculation method.
- Whether exchange rates are set manually or updated automatically.
- Currency used for electronic payments.
- Method used to display prices and reports.

---
## 11. Product and Inventory Management

### 11.1 Product Data

Each product should support:

- Arabic and English names.
- Arabic and English descriptions.
- Multiple images.
- Price.
- Original price before discount.
- Main category and subcategory.
- Sizes.
- Colors.
- Weight.
- Capacity.
- Stock quantity.
- Stock Keeping Unit (**SKU**).
- Product status.
- Featured-product status.
- Discount status.

Not every option is mandatory for every product. For example:

- Clothing requires sizes and colors.
- Bags may require colors and capacity.
- Some products may require weight.
- Some products may have no selectable options.

### 11.2 Inventory Management

The system must support:

- Defining the quantity of each product.
- Defining the quantity of each color or size variant.
- Preventing customers from purchasing more than the available stock.
- Marking a product as unavailable when its quantity reaches zero.
- Alerting administrators when inventory is low.
- Manually adjusting inventory.
- Importing or updating inventory through an Excel file.

### 11.3 Excel File Handling

A feature may be added to import products or inventory from an Excel file.

A standardized Excel template should be prepared with fields such as:

- Product code.
- Product name.
- Size.
- Color.
- Quantity.
- Price.

When a file is uploaded, the system must:

- Validate the data.
- Display errors before saving.
- Update existing products.
- Add new products according to permissions and system settings.

Excel import may be implemented in the first phase if the inventory is large, or postponed if the number of products is limited.

---

## 12. Orders and Order Statuses

Each order must have a unique order number.

Suggested order statuses:

- New Order.
- Under Review.
- Confirmed.
- Being Prepared.
- Ready for Shipping.
- Shipped.
- Delivered.
- Cancelled.
- Returned.
- Payment Failed.

Authorized users must be able to:

- View order details.
- Change the order status.
- Add an internal note.
- View customer information.
- View ordered products and quantities.
- View the payment method.
- Print the order or invoice.
- Export the order to Excel.
- Contact the customer.

---

## 13. Administration Dashboard

The administration dashboard must provide the following sections.

### 13.1 Main Dashboard

The main dashboard should display a summary of:

- Number of new orders.
- Today’s sales.
- Current month’s sales.
- Number of customers.
- Low-stock products.
- Best-selling products.
- Latest orders.
- Important alerts.

### 13.2 Product Management

This section includes:

- Add a product.
- Edit a product.
- Delete or hide a product.
- Manage images.
- Manage prices.
- Manage product options.
- Manage inventory.
- Manage featured products.
- Manage promotions.

### 13.3 Category Management

This section includes:

- Add a main category.
- Add a subcategory.
- Edit category names and images.
- Reorder categories.
- Show or hide a category.

### 13.4 Order Management

This section includes:

- View orders.
- Search and filter orders.
- Change order status.
- View order details.
- Export orders to Excel.
- Print orders.
- Send a notification to the customer.

### 13.5 Customer Management

This section includes:

- View customer information.
- Search for a customer.
- View a customer’s orders.
- Disable an account when necessary.
- Edit selected customer information according to permissions.

### 13.6 Content Management

This section includes:

- Banners.
- Home page content.
- About Us.
- Project Story.
- Delivery Methods.
- Payment Methods.
- Contact Us.
- Policies and terms.
- Social media links.

### 13.7 System Settings

This section includes:

- System name.
- System logo.
- Store information.
- Contact numbers.
- Email address.
- Store address.
- Currency.
- Languages.
- Payment settings.
- OTP settings.
- Notification settings.
- Social media links.
- Search Engine Optimization (**SEO**) settings.
- Copyright text.

---

## 14. Users and Permissions

### 14.1 Main Administrator

The main administrator has all permissions, including:

- Manage users.
- Manage permissions.
- Manage products.
- Manage orders.
- Manage inventory.
- Manage customers.
- Manage content.
- View reports.
- Edit system settings.
- Configure payment methods and integrations.

### 14.2 Data Entry User

The data entry user has limited permissions, such as:

- Add products.
- Edit product data.
- Add images.
- Update inventory.
- Follow up on orders.
- Change selected order statuses.

The data entry user cannot:

- Edit payment settings.
- Delete users.
- Change administrator permissions.
- Edit sensitive settings.
- View sensitive financial reports unless explicitly granted permission.

The system must use a flexible permission model that allows administrators to define what each user can view or edit.

---

## 15. Reports

The following reports are important for management.

### 15.1 Sales Reports

- Daily sales.
- Weekly sales.
- Monthly sales.
- Sales within a selected date range.
- Sales by payment method.
- Sales by order status.

### 15.2 Product Reports

- Best-selling products.
- Least-selling products.
- Out-of-stock products.
- Low-stock products.
- Quantity sold for each product.
- Sales by category.

### 15.3 Customer Reports

- Number of new customers.
- Top customers by purchasing activity.
- Number of orders per customer.
- Total customer purchases.
- Inactive customers.

### 15.4 Order Reports

- Number of new orders.
- Completed orders.
- Cancelled orders.
- Returned orders.
- Orders by city.
- Orders by payment method.

Reports and orders must be exportable to Excel.

---

## 16. Notifications and External Integrations

### 16.1 Administration Notifications

An administration notification should be sent when:

- A new order is received.
- A payment fails.
- A product’s stock becomes low.
- A product goes out of stock.
- A new customer registers, when needed.

### 16.2 Customer Notifications

A notification may be sent to the customer when:

- The order is confirmed.
- The order status changes.
- The order is shipped.
- The order is cancelled.
- Payment succeeds or fails.

### 16.3 Delivery Channels

Notifications may be sent through:

- SMS messages.
- WhatsApp.
- Email.
- In-account notifications.

The external services used for each delivery channel must be defined.

---

## 17. Discount Coupons and Promotions

The system must support discount coupons with the ability to define:

- Coupon code.
- Discount type: percentage or fixed amount.
- Coupon start and end dates.
- Minimum order value.
- Total usage limit.
- Usage limit per customer.
- Included products or categories.
- Enabled or disabled status.

The system must also support:

- Direct product discounts.
- Original price and discounted price.
- Promotions for selected categories.
- Featured products.

---

## 18. Search Engine Optimization (SEO)

Basic SEO must be implemented, including:

- Setting the title of each page.
- Setting the description of each page.
- Creating clear and readable page URLs.
- Setting product titles and descriptions.
- Setting category titles and descriptions.
- Generating a sitemap.
- Configuring the `robots.txt` file.
- Adding alternative text (**alt text**) to images.
- Connecting the website to Google Search Console.
- Supporting product-link sharing on social media platforms.

Administrators must be able to edit page and product titles and descriptions from the administration dashboard.

---
## 19. Cookies and Privacy

A cookie notice must be added to the website.

The notice should include:

- An explanation of how cookies are used.
- An acceptance button.
- A link to the Cookie Policy.
- The ability to manage cookie categories when analytics or marketing tools are used.

A Privacy Policy must also be provided, explaining:

- The data that is collected.
- Why the data is collected.
- How the data is stored.
- The external services being used.
- Customer rights related to their data.

---

## 20. Backup and Security

### 20.1 Backup

A weekly backup must be implemented and should include:

- Database.
- Product images.
- Important system files.
- System settings.

It is preferable to retain multiple backup copies and not store all backups on the same server.

### 20.2 Security

The following security requirements must be considered:

- Password hashing and secure storage.
- Use of HTTPS connections.
- Protection of the administration dashboard.
- Limiting the number of login attempts.
- Expiration of OTP codes.
- Logging important actions performed by administrators.
- Securing file and image uploads.
- Verifying user permissions for every operation.
- Never storing bank card details inside the system.

---

## 21. Training and Testing Environment

Before the store is officially launched, a separate staging environment must be provided apart from the live website.

The staging environment will be used to:

- Test adding products.
- Train the administration team.
- Test orders.
- Test order statuses.
- Test sandbox payments.
- Test OTP functionality.
- Test messages and notifications.
- Discover and fix issues before launch.
- Review the design on mobile phones and computers.

After the system is approved, the final version will be deployed to the production environment.

The staging website must not appear in search engine results.

---

## 22. Footer and Copyright

The website footer must include:

- Store name.
- Store logo.
- A brief description.
- Links to important pages.
- Contact information.
- Social media links.
- Privacy Policy.
- Cookie Policy.
- Terms and Conditions.
- Copyright notice.

The following items must be editable from the system settings:

- System name.
- Logo.
- Copyright text.
- Social media links.
- Showing or hiding each social media platform.
- Phone number.
- Email address.
- Store address.

Social media platforms should be displayed as icons. Clicking an icon should open the store’s page on the corresponding platform.

---

## 23. Suggested Implementation Order

### Phase One: Analysis and Preparation

1. Finalize the store name and identity.
2. Define categories and subcategories.
3. Prepare the product list.
4. Prepare images and product information.
5. Define payment and delivery methods.
6. Define roles and permissions.
7. Approve the store’s appearance and design.

### Phase Two: Design

1. Design the home page.
2. Design the categories page.
3. Design the product page.
4. Design the shopping cart and checkout process.
5. Design login and customer account pages.
6. Design the administration dashboard.
7. Approve the mobile-first design.

### Phase Three: Administration Dashboard Development

1. Users and permissions.
2. Categories and subcategories.
3. Products and options.
4. Inventory.
5. Content and banners.
6. System settings.

### Phase Four: Storefront Development

1. Home page.
2. Category display.
3. Product listings.
4. Product page.
5. Shopping cart.
6. Discount coupons.
7. Checkout.

### Phase Five: Accounts and Payments

1. Registration using a phone number.
2. OTP verification.
3. Login.
4. Password recovery.
5. Customer account.
6. Payment method integration.

### Phase Six: Orders and Notifications

1. Order creation.
2. Order status management.
3. Administration notifications.
4. Customer messages.
5. Order exports.

### Phase Seven: Reports and SEO

1. Sales reports.
2. Product reports.
3. Customer and inventory reports.
4. SEO configuration.
5. Google Search Console integration.

### Phase Eight: Testing and Launch

1. Run the staging environment.
2. Train the store team.
3. Test on mobile phones and computers.
4. Test payments and OTP functionality.
5. Test backups.
6. Resolve feedback and reported issues.
7. Officially launch the store.

---

## 24. Decisions That Must Be Finalized Before Development

Before actual development begins, clear decisions must be made regarding the following points:

1. Must customers create an account to complete an order?
2. Which countries and cities will delivery cover?
3. Does the product price include delivery for all regions?
4. What is the base currency?
5. Will more than one currency be supported in the first phase?
6. Which electronic payment gateway will be used?
7. Which electronic wallet will be used?
8. Which OTP and messaging service will be used?
9. Will the official WhatsApp API be used?
10. Is the tax number mandatory or optional?
11. Is Excel inventory import required in the first phase?
12. Which final order statuses will management adopt?
13. Who will use the administration dashboard, and what permissions will each user have?
14. Which pages and policies must have finalized content before launch?
15. Will the store operate only inside Palestine, or will it sell internationally?

---

## 25. Summary of the Required First Version

The first version of the Matarzat Al Kahf store must provide a complete, mobile-responsive e-commerce experience that displays Palestinian embroidered products in both Arabic and English. It must allow customers to select a product and its options, add it to the shopping cart, complete the order, and make a payment.

It must also provide an administration dashboard for managing products, categories, inventory, orders, customers, and content. The dashboard must support different permissions for administrators and data entry users, along with basic reports, notifications, backups, and a staging environment before launch.
