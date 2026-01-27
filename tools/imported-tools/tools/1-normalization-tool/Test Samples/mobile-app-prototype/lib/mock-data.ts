export interface Employee {
  id: string
  name: string
  role: string
  department: string
  email: string
  phone?: string
  showPhone: boolean
  profileSummary: string
  avatar: string
  dateOfBirth?: string
  homeAddress?: string
  accreditationStatus: string
}

export interface Accreditation {
  id: string
  name: string
  description: string
  status: "Valid" | "Expired" | "Pending"
  expiryDate?: string
  issuedDate?: string
  category: "Internal" | "External"
}

export interface Policy {
  id: string
  title: string
  category: string
  lastUpdated: string
  lastUpdatedBy: string
  content: string
  excerpt: string
}

export const currentUserId = "1"

export const employees: Employee[] = [
  {
    id: "1",
    name: "Jordan Davis",
    role: "Product Manager",
    department: "Product",
    email: "jordan.davis@company.com",
    phone: "+1 (555) 123-4567",
    showPhone: false,
    profileSummary:
      "Passionate about building products that make a difference. 5+ years in product management with focus on B2B SaaS.",
    avatar: "/professional-avatar.png",
    dateOfBirth: "1990-03-15",
    homeAddress: "123 Main St, San Francisco, CA 94102",
    accreditationStatus: "3 Active, 1 Expiring Soon",
  },
  {
    id: "2",
    name: "Sarah Johnson",
    role: "Chief Executive Officer",
    department: "Executive",
    email: "sarah.johnson@company.com",
    showPhone: false,
    profileSummary:
      "Leading with vision and empathy. 15+ years of experience scaling technology companies from startup to enterprise.",
    avatar: "/female-executive.png",
    accreditationStatus: "5 Active",
  },
  {
    id: "3",
    name: "Michael Chen",
    role: "Head of People & Culture",
    department: "HR",
    email: "michael.chen@company.com",
    phone: "+1 (555) 234-5678",
    showPhone: true,
    profileSummary:
      "Building inclusive workplaces where everyone thrives. Certified HR professional with expertise in organizational development.",
    avatar: "/asian-professional.jpg",
    accreditationStatus: "4 Active",
  },
  {
    id: "4",
    name: "Alex Rivera",
    role: "Senior Account Manager",
    department: "Sales",
    email: "alex.rivera@company.com",
    showPhone: false,
    profileSummary:
      "Connecting clients with solutions that drive growth. Award-winning sales professional specializing in enterprise accounts.",
    avatar: "/hispanic-professional.jpg",
    accreditationStatus: "2 Active, 1 Pending",
  },
  {
    id: "5",
    name: "Emily Watson",
    role: "Senior Software Engineer",
    department: "Engineering",
    email: "emily.watson@company.com",
    phone: "+1 (555) 345-6789",
    showPhone: true,
    profileSummary:
      "Full-stack developer passionate about clean code and user experience. 8 years building scalable web applications.",
    avatar: "/tech-woman.jpg",
    accreditationStatus: "4 Active",
  },
  {
    id: "6",
    name: "David Park",
    role: "UX Designer",
    department: "Design",
    email: "david.park@company.com",
    showPhone: false,
    profileSummary: "Designing human-centered experiences. Specializing in mobile-first design and accessibility.",
    avatar: "/designer-male.jpg",
    accreditationStatus: "3 Active",
  },
  {
    id: "7",
    name: "Lisa Thompson",
    role: "Marketing Director",
    department: "Marketing",
    email: "lisa.thompson@company.com",
    phone: "+1 (555) 456-7890",
    showPhone: true,
    profileSummary:
      "Strategic marketer driving brand growth through data-driven campaigns. 10+ years in B2B marketing leadership.",
    avatar: "/marketing-woman.jpg",
    accreditationStatus: "5 Active",
  },
  {
    id: "8",
    name: "James Wilson",
    role: "DevOps Engineer",
    department: "Engineering",
    email: "james.wilson@company.com",
    showPhone: false,
    profileSummary:
      "Infrastructure automation expert. Building reliable, scalable systems with cloud-native technologies.",
    avatar: "/devops-male.jpg",
    accreditationStatus: "3 Active, 1 Expired",
  },
]

export const accreditations: Accreditation[] = [
  {
    id: "1",
    name: "Information Security Awareness",
    description:
      "Comprehensive training on company security policies and best practices for protecting sensitive data.",
    status: "Valid",
    expiryDate: "2024-12-31",
    issuedDate: "2024-01-15",
    category: "Internal",
  },
  {
    id: "2",
    name: "Workplace Safety Certification",
    description: "Training on workplace safety protocols, emergency procedures, and health regulations.",
    status: "Valid",
    expiryDate: "2025-03-15",
    issuedDate: "2024-03-15",
    category: "Internal",
  },
  {
    id: "3",
    name: "Project Management Professional (PMP)",
    description: "Industry-recognized certification demonstrating expertise in project management methodologies.",
    status: "Valid",
    expiryDate: "2025-06-30",
    issuedDate: "2022-06-30",
    category: "External",
  },
  {
    id: "4",
    name: "Certified ScrumMaster (CSM)",
    description: "Agile methodology certification for facilitating Scrum teams and implementing agile practices.",
    status: "Pending",
    category: "External",
  },
  {
    id: "5",
    name: "Data Privacy & GDPR Compliance",
    description: "Training on data protection regulations and privacy best practices.",
    status: "Expired",
    expiryDate: "2023-11-30",
    issuedDate: "2022-11-30",
    category: "Internal",
  },
  {
    id: "6",
    name: "First Aid & CPR",
    description: "Emergency response training including CPR, AED use, and basic first aid procedures.",
    status: "Valid",
    expiryDate: "2025-08-20",
    issuedDate: "2023-08-20",
    category: "External",
  },
  {
    id: "7",
    name: "Diversity & Inclusion Training",
    description: "Comprehensive program on building inclusive workplaces and unconscious bias awareness.",
    status: "Valid",
    expiryDate: "2024-12-31",
    issuedDate: "2024-01-10",
    category: "Internal",
  },
]

export const policies: Policy[] = [
  {
    id: "1",
    title: "Remote Work Policy",
    category: "Work Arrangements",
    lastUpdated: "2024-01-15",
    lastUpdatedBy: "Michael Chen",
    excerpt: "Guidelines for remote work eligibility, expectations, and best practices for distributed teams.",
    content: `## Purpose

This policy establishes guidelines for employees who work remotely, either full-time or on a hybrid schedule. Our goal is to maintain productivity, collaboration, and work-life balance while supporting flexible work arrangements.

## Eligibility

All full-time employees are eligible for remote work arrangements, subject to:
- Manager approval
- Role compatibility with remote work
- Demonstrated ability to work independently
- Reliable internet connection and appropriate workspace

## Expectations

### Working Hours
- Maintain consistent core hours (10 AM - 3 PM local time)
- Be available for scheduled meetings and team collaboration
- Communicate your schedule with your team

### Communication
- Respond to messages within 2 hours during working hours
- Use video for team meetings when possible
- Keep your status updated in communication tools

### Security
- Use company-approved VPN for accessing internal resources
- Keep work devices secure and up-to-date
- Never share credentials or access with others

## Equipment & Support

The company provides:
- Laptop and necessary peripherals
- Home office stipend ($500 annually)
- Technical support for company-issued equipment

## Review

Remote work arrangements are reviewed quarterly with managers to ensure they continue to meet business needs and employee preferences.`,
  },
  {
    id: "2",
    title: "Code of Conduct",
    category: "Ethics & Compliance",
    lastUpdated: "2024-02-01",
    lastUpdatedBy: "Sarah Johnson",
    excerpt: "Our standards for professional behavior, ethical conduct, and workplace interactions.",
    content: `## Our Values

We are committed to maintaining a professional, respectful, and inclusive workplace. This Code of Conduct outlines expected behaviors and provides guidance for decision-making.

## Professional Standards

### Respect & Inclusion
- Treat all colleagues, clients, and partners with dignity and respect
- Value diverse perspectives and experiences
- Create an environment where everyone feels welcome

### Integrity
- Act honestly and ethically in all business dealings
- Avoid conflicts of interest
- Report any violations of policy or law

### Collaboration
- Work cooperatively with colleagues
- Share knowledge and support team success
- Communicate openly and constructively

## Unacceptable Behavior

The following behaviors are prohibited:
- Harassment or discrimination of any kind
- Violence or threats
- Theft or fraud
- Sharing confidential information
- Substance abuse at work

## Reporting Concerns

If you witness or experience behavior that violates this code:
1. Speak directly with the person if you feel comfortable
2. Report to your manager or HR
3. Use the anonymous hotline: 1-800-ETHICS-1
4. Email: ethics@company.com

All reports are taken seriously and investigated promptly. Retaliation against those who report concerns is strictly prohibited.`,
  },
  {
    id: "3",
    title: "Time Off & Leave Policy",
    category: "Benefits & Leave",
    lastUpdated: "2024-01-20",
    lastUpdatedBy: "Michael Chen",
    excerpt: "Comprehensive guide to vacation time, sick leave, parental leave, and other time-off benefits.",
    content: `## Vacation Time

### Accrual
- 0-2 years: 15 days per year
- 3-5 years: 20 days per year
- 6+ years: 25 days per year

### Usage Guidelines
- Request time off at least 2 weeks in advance
- Coordinate with your team to ensure coverage
- Maximum carryover: 5 days to next year

## Sick Leave

- Unlimited sick days for genuine illness
- No doctor's note required for absences under 3 consecutive days
- Notify your manager as soon as possible

## Parental Leave

### Birth Parents
- 16 weeks paid leave
- Can be taken continuously or split within first year

### Non-Birth Parents
- 12 weeks paid leave
- Can be taken continuously or split within first year

## Other Leave Types

### Bereavement Leave
- Up to 5 days for immediate family
- Up to 3 days for extended family

### Jury Duty
- Fully paid while serving
- Company continues benefits

### Personal Days
- 5 days per year for personal matters
- No reason required

## Requesting Time Off

1. Submit request in the HR system
2. Await manager approval
3. Add to team calendar
4. Set up out-of-office messages`,
  },
  {
    id: "4",
    title: "Information Security Policy",
    category: "Security & Privacy",
    lastUpdated: "2024-01-10",
    lastUpdatedBy: "James Wilson",
    excerpt: "Critical security practices and requirements for protecting company and client data.",
    content: `## Purpose

Information security is everyone's responsibility. This policy establishes requirements for protecting company and client data from unauthorized access, disclosure, or loss.

## Password Requirements

- Minimum 12 characters
- Include uppercase, lowercase, numbers, and symbols
- Change every 90 days
- Never reuse previous passwords
- Use password manager for secure storage

## Device Security

### Company Devices
- Enable full disk encryption
- Install security updates within 48 hours
- Lock screen when unattended
- Report lost/stolen devices immediately

### Personal Devices (BYOD)
- Install approved mobile device management (MDM)
- Separate work and personal data
- Allow remote wipe capability

## Data Classification

### Public
- Marketing materials
- Public website content
- Published reports

### Internal
- Employee directory
- Internal communications
- Most business documents

### Confidential
- Customer data
- Financial information
- Product roadmaps
- Source code

### Restricted
- Social security numbers
- Payment card data
- Health information
- Trade secrets

## Best Practices

- Use VPN for public WiFi
- Verify sender before clicking links
- Report suspicious emails to security@company.com
- Encrypt confidential data
- Use secure file sharing (never email sensitive data)

## Incident Reporting

If you suspect a security incident:
1. Disconnect affected device from network
2. Contact IT Security: security@company.com or ext. 7777
3. Document what happened
4. Do not attempt to investigate yourself`,
  },
  {
    id: "5",
    title: "Expense Reimbursement Policy",
    category: "Finance & Travel",
    lastUpdated: "2023-12-15",
    lastUpdatedBy: "Lisa Thompson",
    excerpt: "How to submit and get reimbursed for business expenses and travel costs.",
    content: `## Eligible Expenses

The company reimburses reasonable business expenses including:
- Travel (flights, hotels, ground transportation)
- Client meals and entertainment
- Office supplies
- Professional development
- Conference fees

## Limits & Guidelines

### Meals
- Breakfast: $25
- Lunch: $35
- Dinner: $75
- Client dinners: Pre-approval required for >$150/person

### Hotels
- Use corporate booking tool
- Standard room rate (not suites)
- Reasonable location near business activities

### Transportation
- Economy class for flights under 5 hours
- Business class for flights 5+ hours
- Rental cars: Mid-size or smaller
- Rideshare/taxi: Reasonable for business use

## Submission Process

1. Make purchase with corporate card or personal funds
2. Keep itemized receipts
3. Submit in expense system within 30 days
4. Include business purpose and attendees
5. Manager reviews and approves
6. Reimbursement within 2 weeks

## Corporate Credit Card

### Usage
- For business expenses only
- Never for personal purchases
- Report lost/stolen immediately

### Reconciliation
- Due 5 days after statement close
- Attach receipts for all expenses
- Late submissions may delay reimbursement

## Non-Reimbursable

- Personal entertainment
- Alcohol without client present
- Excessive or luxury purchases
- Parking tickets or traffic violations
- Commuting costs`,
  },
  {
    id: "6",
    title: "Professional Development Policy",
    category: "Learning & Growth",
    lastUpdated: "2024-01-25",
    lastUpdatedBy: "Michael Chen",
    excerpt: "Annual learning budget, conference attendance, and career development opportunities.",
    content: `## Learning Budget

Every employee receives an annual professional development budget:
- Individual Contributors: $2,000
- Managers: $3,000
- Senior Leaders: $5,000

## Eligible Activities

### Courses & Certifications
- Online courses (Coursera, LinkedIn Learning, etc.)
- Professional certifications
- Degree programs (with additional approval)

### Conferences & Events
- Industry conferences
- Professional meetups
- Workshops and seminars

### Books & Resources
- Technical books
- Industry publications
- Online learning subscriptions

## Approval Process

### Under $500
- Submit request to manager
- Approval typically within 3 days

### $500-$2,000
- Submit with business justification
- Manager and Director approval required

### Over $2,000
- Requires VP approval
- Must show clear business value

## Time Off for Learning

- Up to 5 days per year for courses/conferences
- Counts as work time, not vacation
- Schedule around team needs

## Knowledge Sharing

Recipients are encouraged to:
- Present key learnings to team
- Write summary for company knowledge base
- Mentor others in new skills

## Tuition Reimbursement

For degree programs:
- 50% reimbursement up to $10,000/year
- Must be relevant to current or future role
- Requires C grade or better
- 2-year commitment to company after completion`,
  },
]

export function getEmployeeById(id: string): Employee | undefined {
  return employees.find((emp) => emp.id === id)
}

export function getAccreditationById(id: string): Accreditation | undefined {
  return accreditations.find((acc) => acc.id === id)
}

export function getPolicyById(id: string): Policy | undefined {
  return policies.find((policy) => policy.id === id)
}
